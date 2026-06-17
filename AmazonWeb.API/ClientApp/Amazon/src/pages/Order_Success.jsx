import React from 'react';
import { Link, useLocation } from 'react-router-dom';

const Order_Success = () => {
  const location = useLocation();
  
  // 🎯 PARSE QUERY PARAMETERS: Extracts 'orderNo' directly from '?orderNo=...'
  const queryParams = new URLSearchParams(location.search);
  const orderId = queryParams.get('orderNo') || "8159aedb-94f8-4305-bd8f-ae1767590a99";
  
  // Optional fallback value or secondary query parameter if you choose to append it later
  const totalAmount = queryParams.get('totalAmount') || "0";

  return (
    <div className="order-success-fluid-container">
      <div className="order-success-card-panel">
        
        {/* 1. Primary Success Hero Header */}
        <div className="success-header-banner">
          <div className="success-checkmark-circle">✓</div>
          <div className="success-title-text-block">
            <h1>Order placed, thank you!</h1>
            <p className="success-delivery-subtext">
              Confirmation will be sent to your registered email account.
            </p>
          </div>
        </div>

        <hr className="success-layout-divider" />

        {/* 2. Order Metadata Info Details Display */}
        <div className="success-metadata-summary-box">
          <div className="metadata-row-item">
            <span className="metadata-label">Order Number:</span>
            <span className="metadata-value identifier-hash">{orderId}</span>
          </div>
          {totalAmount > 0 && (
            <div className="metadata-row-item">
              <span className="metadata-label">Total Paid:</span>
              <span className="metadata-value amount-text-amber">₹{totalAmount}</span>
            </div>
          )}
        </div>

        {/* 3. Re-engagement & Next Action Navigation Flows */}
        <div className="success-actions-redirection-tray">
          <Link to="/orders" className="amazon-pill-btn-success secondary-gray">
            Review your orders
          </Link>
          <Link to="/" className="amazon-pill-btn-success primary-gold">
            Continue Shopping
          </Link>
        </div>

        {/* 4. Downstream Upsell/Security Compliance Legal Disclaimer */}
        <div className="success-security-disclaimer border-top-split">
          <p>
            Need to make changes to this shipment? You can track, modify, or cancel items up until they enter the shipping process via your account management dashboard. 
            Go to <Link to="/account">Your Account</Link> to check ongoing fulfillment status updates.
          </p>
        </div>

      </div>
    </div>
  );
};

export default Order_Success;