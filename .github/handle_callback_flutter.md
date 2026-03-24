```dart
/// Model kết quả thanh toán VNPAY trả về qua Return URL.
class VNPayResult {
  final String? responseCode;
  final String? transactionStatus;
  final String? txnRef;
  final String? transactionNo;
  final String? amount;
  final String? bankCode;
  final String? payDate;
  final bool isSuccess;
  final String message;

  const VNPayResult({
    required this.responseCode,
    required this.transactionStatus,
    required this.txnRef,
    required this.transactionNo,
    required this.amount,
    required this.bankCode,
    required this.payDate,
    required this.isSuccess,
    required this.message,
  });

  /// Parse từ Return URL mà VNPAY redirect về.
  factory VNPayResult.fromUrl(String url) {
    // URL dạng myapp://payment-result?vnp_ResponseCode=00&...
    // Uri.parse xử lý được cả custom scheme
    final uri = Uri.parse(url);
    final p = uri.queryParameters;

    final responseCode = p['vnp_ResponseCode'];
    final transactionStatus = p['vnp_TransactionStatus'];

    final success = responseCode == '00' && transactionStatus == '00';

    return VNPayResult(
      responseCode: responseCode,
      transactionStatus: transactionStatus,
      txnRef: p['vnp_TxnRef'],
      transactionNo: p['vnp_TransactionNo'],
      amount: p['vnp_Amount'],
      bankCode: p['vnp_BankCode'],
      payDate: p['vnp_PayDate'],
      isSuccess: success,
      message: _resolveMessage(responseCode, transactionStatus),
    );
  }

  /// Số tiền hiển thị (VNĐ) — VNPAY trả về giá trị đã nhân 100
  String get displayAmount {
    if (amount == null) return '-';
    final raw = int.tryParse(amount!);
    if (raw == null) return '-';
    return '${(raw ~/ 100).toString().replaceAllMapped(
          RegExp(r'(\d)(?=(\d{3})+(?!\d))'),
          (m) => '${m[1]}.',
        )} VNĐ';
  }

  static String _resolveMessage(
      String? responseCode, String? transactionStatus) {
    if (responseCode == '00' && transactionStatus == '00') {
      return 'Thanh toán thành công';
    }
    switch (responseCode) {
      case '07':
        return 'Trừ tiền thành công nhưng giao dịch bị nghi ngờ gian lận';
      case '09':
        return 'Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking';
      case '10':
        return 'Xác thực thông tin thẻ/tài khoản quá 3 lần';
      case '11':
        return 'Đã hết hạn chờ thanh toán';
      case '12':
        return 'Thẻ/Tài khoản bị khoá';
      case '13':
        return 'Sai OTP — vui lòng thử lại';
      case '24':
        return 'Người dùng đã hủy giao dịch';
      case '51':
        return 'Tài khoản không đủ số dư';
      case '65':
        return 'Vượt quá hạn mức giao dịch trong ngày';
      case '75':
        return 'Ngân hàng đang bảo trì';
      case '79':
        return 'Nhập sai mật khẩu thanh toán quá số lần cho phép';
      default:
        return 'Thanh toán thất bại (mã: ${responseCode ?? "?"})';
    }
  }
}
```
