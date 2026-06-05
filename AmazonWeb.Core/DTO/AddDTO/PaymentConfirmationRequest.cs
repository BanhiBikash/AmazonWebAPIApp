public class PaymentConfirmationRequest
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public string RazorpayPaymentId { get; set; } = string.Empty;

    public string? RazorpayOrderId { get; set; }

    [Required]
    public string RazorpaySignature { get; set; } = string.Empty;
}