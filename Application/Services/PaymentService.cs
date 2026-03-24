using Application.DTOs.ResponseDTOs.Payment;
using Application.DTOs.RequestDTOs.Payment;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using Domain.Const;

namespace Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePaymentUrlResponse> CreateVnPayPaymentUrlAsync(
        Guid userId,
        CreateVnPayPaymentUrlRequest request,
        string? clientIp)
    {
        if (request.AmountVnd <= 0)
            throw new Exception("AmountVnd must be greater than 0.");

        request.OrderInfo = request.TxnRef;
        if (string.IsNullOrWhiteSpace(request.OrderInfo))
            throw new Exception("OrderInfo is required.");

        if (string.IsNullOrWhiteSpace(request.TxnRef))
            throw new Exception("TxnRef is required.");

        if (!Guid.TryParse(request.TxnRef, out var orderId))
            throw new Exception("TxnRef must be a valid order id.");

        var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId)
            ?? throw new Exception("Order not found.");

        if (order.UserId != userId)
            throw new Exception("You are not allowed to pay this order.");

        if (!string.Equals(order.PaymentMethod, "VNPAY", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Order payment method is not VNPAY.");

        if (string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Order is already paid.");

        if ((order.TotalAmount ?? 0) <= 0)
            throw new Exception("Invalid order amount.");

        var expectedAmountVnd = (int)Math.Round(order.TotalAmount!.Value, MidpointRounding.AwayFromZero);
        if (request.AmountVnd != expectedAmountVnd)
            throw new Exception("AmountVnd does not match order total amount.");

        var ipAddr = string.IsNullOrWhiteSpace(request.IpAddr) ? clientIp : request.IpAddr;

        var paymentUrl = VnPay.BuildPaymentUrl(
            request.AmountVnd,
            request.OrderInfo,
            request.TxnRef,
            ipAddr);

        return new CreatePaymentUrlResponse
        {
            OrderId = order.Id,
            Amount = order.TotalAmount.Value,
            PaymentMethod = "VNPAY",
            PaymentStatus = order.PaymentStatus ?? "Pending",
            PaymentUrl = paymentUrl
        };
    }

    public async Task<CreatePaymentUrlResponse> CreateVnPayBookingPaymentUrlAsync(
        Guid userId,
        CreateVnPayPaymentUrlRequest request,
        string? clientIp)
    {
        if (request.AmountVnd <= 0)
            throw new Exception("AmountVnd must be greater than 0.");

        if (string.IsNullOrWhiteSpace(request.TxnRef))
            throw new Exception("TxnRef is required.");

        if (!Guid.TryParse(request.TxnRef, out var bookingId))
            throw new Exception("TxnRef must be a valid booking id.");

        var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.UserId != userId)
            throw new Exception("You are not allowed to pay this booking.");

        if (booking.IsPaid == true)
            throw new Exception("Booking is already paid.");

        if ((booking.TotalPrice ?? 0) <= 0)
            throw new Exception("Invalid booking amount.");

        var expectedAmountVnd = (int)Math.Round(booking.TotalPrice!.Value, MidpointRounding.AwayFromZero);
        if (request.AmountVnd != expectedAmountVnd)
            throw new Exception("AmountVnd does not match booking total amount.");

        var ipAddr = string.IsNullOrWhiteSpace(request.IpAddr) ? clientIp : request.IpAddr;

        var paymentUrl = VnPay.BuildPaymentUrl(
            request.AmountVnd,
            request.TxnRef,
            request.TxnRef,
            ipAddr);

        return new CreatePaymentUrlResponse
        {
            OrderId = booking.Id,
            Amount = booking.TotalPrice.Value,
            PaymentMethod = "VNPAY",
            PaymentStatus = booking.IsPaid == true ? "Paid" : "Pending",
            PaymentUrl = paymentUrl
        };
    }

    public async Task<object> HandleVnPayIpnAsync(IReadOnlyDictionary<string, string> queryParams)
    {
        if (!VnPay.VerifySignature(queryParams))
        {
            return new { RspCode = "97", Message = "Invalid signature" };
        }

        var txnRef = GetValue(queryParams, "vnp_TxnRef");
        var amountRaw = GetValue(queryParams, "vnp_Amount");
        var responseCode = GetValue(queryParams, "vnp_ResponseCode");
        var transactionStatus = GetValue(queryParams, "vnp_TransactionStatus");

        if (!Guid.TryParse(txnRef, out var entityId))
        {
            return new { RspCode = "01", Message = "Transaction reference not found" };
        }

        // Try Order first
        var order = await _unitOfWork.OrderRepository.GetByIdAsync(entityId);
        if (order != null)
        {
            var expectedAmount = ((long)Math.Round((order.TotalAmount ?? 0) * 100M, MidpointRounding.AwayFromZero)).ToString();
            if (!string.Equals(expectedAmount, amountRaw, StringComparison.Ordinal))
                return new { RspCode = "04", Message = "Invalid amount" };

            if (string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                await ReconcileOrderAfterPaymentSuccessAsync(order);
                _unitOfWork.OrderRepository.Update(order);
                await _unitOfWork.SaveChangesAsync();
                return new { RspCode = "02", Message = "Order already confirmed" };
            }

            var success = IsPaymentSuccess(responseCode, transactionStatus);
            order.PaymentStatus = success ? "Paid" : "Failed";
            if (success)
                await ReconcileOrderAfterPaymentSuccessAsync(order);

            _unitOfWork.OrderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return new { RspCode = "00", Message = "Confirm Success" };
        }

        // Try Booking
        var booking = await _unitOfWork.BookingRepository.GetByIdAsync(entityId);
        if (booking != null)
        {
            var expectedAmount = ((long)Math.Round((booking.TotalPrice ?? 0) * 100M, MidpointRounding.AwayFromZero)).ToString();
            if (!string.Equals(expectedAmount, amountRaw, StringComparison.Ordinal))
                return new { RspCode = "04", Message = "Invalid amount" };

            if (booking.IsPaid == true)
                return new { RspCode = "02", Message = "Booking already paid" };

            var success = IsPaymentSuccess(responseCode, transactionStatus);
            if (success)
            {
                booking.IsPaid = true;
                if (string.Equals(booking.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                    booking.Status = "Confirmed";
            }

            _unitOfWork.BookingRepository.Update(booking);
            await _unitOfWork.SaveChangesAsync();
            return new { RspCode = "00", Message = "Confirm Success" };
        }

        return new { RspCode = "01", Message = "Transaction not found" };
    }

    public async Task<VnPayResultResponse> HandleVnPayReturnAsync(IReadOnlyDictionary<string, string> queryParams)
    {
        if (!VnPay.VerifySignature(queryParams))
        {
            return new VnPayResultResponse
            {
                IsSuccess = false,
                Message = "Sai chu ky bao mat.",
                ResponseCode = GetValue(queryParams, "vnp_ResponseCode"),
                TransactionStatus = GetValue(queryParams, "vnp_TransactionStatus"),
                TxnRef = GetValue(queryParams, "vnp_TxnRef"),
                TransactionNo = GetValue(queryParams, "vnp_TransactionNo"),
                Amount = GetValue(queryParams, "vnp_Amount"),
                BankCode = GetValue(queryParams, "vnp_BankCode"),
                PayDate = GetValue(queryParams, "vnp_PayDate")
            };
        }

        // Keep this idempotent and safe: return callback may arrive before/after IPN.
        await HandleVnPayIpnAsync(queryParams);

        var responseCode = GetValue(queryParams, "vnp_ResponseCode");
        var transactionStatus = GetValue(queryParams, "vnp_TransactionStatus");
        var isSuccess = IsPaymentSuccess(responseCode, transactionStatus);

        return new VnPayResultResponse
        {
            IsSuccess = isSuccess,
            Message = ResolveMessage(responseCode, transactionStatus),
            ResponseCode = responseCode,
            TransactionStatus = transactionStatus,
            TxnRef = GetValue(queryParams, "vnp_TxnRef"),
            TransactionNo = GetValue(queryParams, "vnp_TransactionNo"),
            Amount = GetValue(queryParams, "vnp_Amount"),
            BankCode = GetValue(queryParams, "vnp_BankCode"),
            PayDate = GetValue(queryParams, "vnp_PayDate")
        };
    }

    private static bool IsPaymentSuccess(string? responseCode, string? transactionStatus)
    {
        return string.Equals(responseCode, "00", StringComparison.OrdinalIgnoreCase)
               && string.Equals(transactionStatus, "00", StringComparison.OrdinalIgnoreCase);
    }

    private async Task ReconcileOrderAfterPaymentSuccessAsync(Domain.Entities.Order order)
    {
        if (string.Equals(order.OrderStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            order.OrderStatus = "Confirmed";

        var orderDetails = await _unitOfWork.OrderDetailRepository.GetByOrderIdAsync(order.Id);
        var hasOrderDetails = orderDetails.Any();

        var cart = await _unitOfWork.CartRepository.GetByUserIdWithIncludesAsync(order.UserId);

        if (!hasOrderDetails && cart is { CartItems.Count: > 0 })
        {
            decimal totalAmount = 0;

            foreach (var cartItem in cart.CartItems)
            {
                var quantity = cartItem.Quantity ?? 0;
                if (quantity <= 0)
                    continue;

                var product = await _unitOfWork.ProductRepository.GetByIdAsync(cartItem.ProductId)
                    ?? throw new Exception($"Product {cartItem.ProductId} not found.");

                await _unitOfWork.OrderDetailRepository.CreateAsync(new Domain.Entities.OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                });

                totalAmount += product.Price * quantity;
            }

            if (totalAmount > 0)
                order.TotalAmount = totalAmount;
        }

        if (cart is { CartItems.Count: > 0 })
        {
            foreach (var cartItem in cart.CartItems)
            {
                _unitOfWork.CartItemRepository.Delete(cartItem);
            }

            cart.UpdatedAt = DateTime.Now;
            _unitOfWork.CartRepository.Update(cart);
        }
    }

    private static string? GetValue(IReadOnlyDictionary<string, string> queryParams, string key)
    {
        return queryParams.TryGetValue(key, out var value) ? value : null;
    }

    private static string ResolveMessage(string? responseCode, string? transactionStatus)
    {
        if (IsPaymentSuccess(responseCode, transactionStatus))
            return "Thanh toan thanh cong";

        return responseCode switch
        {
            "07" => "Tru tien thanh cong nhung giao dich bi nghi ngo gian lan",
            "09" => "The/Tai khoan chua dang ky Internet Banking",
            "10" => "Xac thuc qua 3 lan",
            "11" => "Da het han cho thanh toan",
            "12" => "The/Tai khoan bi khoa",
            "13" => "Sai OTP",
            "24" => "Nguoi dung huy giao dich",
            "51" => "Tai khoan khong du so du",
            "65" => "Vuot han muc giao dich",
            "75" => "Ngan hang dang bao tri",
            "79" => "Nhap sai mat khau qua so lan cho phep",
            _ => $"Thanh toan that bai (ma: {responseCode ?? "?"})"
        };
    }
}
