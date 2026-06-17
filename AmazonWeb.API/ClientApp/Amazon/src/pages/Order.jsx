import React, { use, useState } from 'react';
import { useLocation, Link, useNavigate } from 'react-router-dom';
import { useContext } from 'react';
import UserContext from '../context/UserContext';
import api from '../api/axiosConfig';

const Order = () => {
  const location = useLocation();
  const {user} = useContext(UserContext);
  const navigate = useNavigate();

  //send to login if not logged in 
  if(!user){
    navigate('/login')
  }

  // 1. Manage order detail local state so updates render instantly on screen
  const [orderData, setOrderData] = useState(location.state?.orderData);
  
  // 2. Inline form management toggles and state maps matching OrderUpdateRequest
  const [isEditing, setIsEditing] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false); 
  const [formError, setFormError] = useState(null);
  
  const [formData, setFormData] = useState({
    id: orderData?.id || '',
    shippingAddress: orderData?.shippingAddress || '',
    city: orderData?.city || '',
    postalCode: orderData?.postalCode || '',
    country: orderData?.country || 'India',
    status: orderData?.orderStatus || 'Pending' 
  });

  if (!orderData) {
    return (
      <div className="orders-error-panel-boundary">
        <div className="error-alert-box">
          <h5>No Order Data Found</h5>
          <p>We couldn't retrieve the details for this order. Please return to your order history tab.</p>
          <Link to="/orders" className="action-btn-pill-small" style={{ marginTop: '12px', display: 'inline-block' }}>
            ← Back to Your Orders
          </Link>
        </div>
      </div>
    );
  }

  // Helper function for status badges matching your OrderStatus options
  const getStatusBadgeClass = (status) => {
    switch (status?.toLowerCase()) {
      case 'pending':
      case 'processing': return 'status-badge-blue';
      case 'shipped': return 'status-badge-amber';
      case 'delivered': return 'status-badge-green';
      case 'cancelled':
      case 'failed': return 'status-badge-red';
      default: return 'status-badge-gray';
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return "N/A";
    const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString(undefined, options);
  };

  // Status restriction conditions
  const currentStatus = orderData.orderStatus?.toLowerCase();
  const isEditable = currentStatus === 'pending' || currentStatus === 'processing';
  
  // Cancel Order validation constraint: Only active if NOT delivered, cancelled, or failed
  const isCancellable = currentStatus !== 'delivered' && currentStatus !== 'cancelled' && currentStatus !== 'failed';

  // Handle local text inputs
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  // Submit address update request to ASP.NET Core API
  const handleFormSubmit = async (e) => {
    e.preventDefault();
    setFormError(null);

    if (formData.shippingAddress.length > 200) {
      setFormError("Shipping address cannot exceed 200 characters.");
      return;
    }
    if (formData.postalCode.length !== 6) {
      setFormError("Postal code should be of 6 digits.");
      return;
    }

    try {
      setIsSubmitting(true);
      
      const updatePayload = {
        Id: formData.id,
        ShippingAddress: formData.shippingAddress,
        PostalCode: formData.postalCode,
        City: formData.city,
        Country: formData.country,
        Status: formData.status 
      };

      //send update request
      const response = await api.post('v1/orders/UpdateOrder', updatePayload);
      
      //store data in a const
      const updatedOrder = response.data;

      if(response.data){
        //set data
      setOrderData(prev => ({
        ...prev,
        shippingAddress: updatedOrder.shippingAddress,
        city: updatedOrder.city,
        postalCode: updatedOrder.postalCode,
        country: updatedOrder.country
      }));

      setIsEditing(false);
      setIsSubmitting(false);
      }else{
        console.error("Failed to receive response data.")
      }
    } catch (err) {
      console.error("API Error updating shipment destination details:", err);
      setFormError(err.response?.data || "Failed to update address.");
      setIsSubmitting(false);
    }
  };

  // 🎯 Cancel Action handler logic sending update payload with status set to "Cancelled"
  const handleCancelOrder = async () => {
    const confirmCancel = window.confirm("Are you sure you want to cancel this order? This action cannot be undone.");
    if (!confirmCancel) return;

    try {
      setIsCancelling(true);

      // 🎯 Matches your backend public OrderUpdateRequest class properties exactly
      const cancelPayload = {
        Id: orderData.id,
        ShippingAddress: orderData.shippingAddress,
        PostalCode: orderData.postalCode,
        City: orderData.city,
        Country: orderData.country,
        Status: "Cancelled" // Explicit status modifier mapped to backend enum name/index
      };

      //make request and store the response
      const response = await api.post('v1/orders/UpdateOrder', cancelPayload);

      //deconstruct and store the status reponse
      const {status} = response.data;

      // Instantly refresh localized context to display visual changes
      setOrderData(prev => ({
        ...prev,
        orderStatus: status
      }));

      // Keep form inline state synced as well
      setFormData(prev => ({
        ...prev,
        status: 'Cancelled'
      }));

      setIsCancelling(false);
    } catch (err) {
      console.error("Failed to abort transaction record state:", err);
      alert(err.response?.data || "Could not cancel order at this stage.");
      setIsCancelling(false);
    }
  };

  return (
    <div className="single-order-detail-fluid-container">
      {/* Breadcrumb navigation links */}
      <nav className="orders-breadcrumb-trail">
        <Link to="/account">Your Account</Link> › <Link to="/orders">Your Orders</Link> › <span className="active-trail">Order Details</span>
      </nav>

      <div className="single-order-meta-title-row">
        <h1>Order Details</h1>
        <p className="meta-subtitle-details">
          Ordered on {formatDate(orderData.ordeDate || orderData.orderDate)} <span className="divider-bar">|</span> Order ID: <span className="mono-id">{orderData.id}</span>
        </p>
      </div>

      {/* Grid Summary Panel: Shipping, Payment, and Summary snapshots */}
      <div className="order-summary-top-card-grid">
        
        {/* Box 1: Dynamic Shipping Destination Panel / Form */}
        <div className="summary-grid-card unique-flex-layout-card address-card-box-override">
          {!isEditing ? (
            <>
              <div className="card-top-content-area">
                <h3>Shipping Address</h3>
                <div className="card-inner-address-text">
                  <p className="shipping-user-name">Fulfillment Delivery</p>
                  <p>{orderData.shippingAddress}</p>
                  <p>{orderData.city}, {orderData.postalCode}</p>
                  <p>{orderData.country || "India"}</p>
                </div>
              </div>
              
              <div className="card-bottom-action-tray">
                <button 
                  className={`amazon-address-update-btn btn-accent-blue ${!isEditable ? 'btn-disabled-state' : ''}`}
                  disabled={!isEditable}
                  onClick={() => setIsEditing(true)}
                  title={isEditable ? "Change delivery destination particulars" : "Addresses cannot be modified once an order has left processing status"}
                >
                  Update Address
                </button>
              </div>
            </>
          ) : (
            <form onSubmit={function(e){handleFormSubmit(e)}} className="inline-address-update-form">
              <h3>Edit Shipping Address</h3>
              {formError && <div className="inline-form-error-toast">{formError}</div>}
              
              <div className="form-input-group-row">
                <label>Street Address</label>
                <input 
                  type="text" 
                  name="shippingAddress"
                  value={formData.shippingAddress} 
                  onChange={handleInputChange}
                  required
                  maxLength={200}
                />
              </div>

              <div className="form-input-split-two-col">
                <div className="form-input-group-row">
                  <label>City</label>
                  <input 
                    type="text" 
                    name="city"
                    value={formData.city} 
                    onChange={handleInputChange}
                    required
                    maxLength={100}
                  />
                </div>
                <div className="form-input-group-row">
                  <label>Postal Code</label>
                  <input 
                    type="text" 
                    name="postalCode"
                    value={formData.postalCode} 
                    onChange={handleInputChange}
                    required
                    maxLength={20}
                  />
                </div>
              </div>

              <div className="form-input-group-row">
                <label>Country</label>
                <input 
                  type="text" 
                  name="country"
                  value={formData.country} 
                  onChange={handleInputChange}
                  required
                  maxLength={100}
                />
              </div>

              <div className="form-actions-pill-row-tray">
                <button 
                  type="button" 
                  className="inline-form-btn secondary-gray" 
                  onClick={() => setIsEditing(false)}
                  disabled={isSubmitting}
                >
                  Cancel
                </button>
                <button 
                  type="submit" 
                  className="inline-form-btn btn-accent-blue-solid"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? "Saving..." : "Save Changes"}
                </button>
              </div>
            </form>
          )}
        </div>

        {/* Box 2: Payment Execution Event Tracking + Cancel Order */}
        <div className="summary-grid-card unique-flex-layout-card">
          <div className="card-top-content-area">
            <h3>Payment Method</h3>
            <div className="card-inner-address-text">
              <p className="payment-method-row">
                <span className="bullet-dot">✓</span> Digital Electronic Transaction Verified
              </p>
              <p className="payment-status-badge-label">
                Status: <span className={`status-pill-indicator ${getStatusBadgeClass(orderData.orderStatus)}`}>{orderData.orderStatus}</span>
              </p>
            </div>
          </div>
          
          <div className="card-bottom-action-tray">
            <button
              type="button"
              className={`amazon-order-cancel-btn btn-accent-red ${!isCancellable ? 'btn-disabled-state' : ''}`}
              disabled={!isCancellable || isCancelling}
              onClick={handleCancelOrder}
              title={isCancellable ? "Abort this transaction shipment entirely" : "Delivered, completed or aborted invoices cannot be cancelled"}
            >
              {isCancelling ? "Cancelling..." : "Cancel Order"}
            </button>
          </div>
        </div>

        {/* Box 3: Cost Accounting Invoice Balance Summary */}
        <div className="summary-grid-card cost-calculation-summary-panel">
          <h3>Order Summary</h3>
          <div className="invoice-rows-wrapper">
            <div className="invoice-row">
              <span>Items Subtotal:</span>
              <span>₹{orderData.totalAmount}</span>
            </div>
            <div className="invoice-row">
              <span>Shipping &amp; Handling:</span>
              <span className="green-free">₹0 (FREE)</span>
            </div>
            <hr className="invoice-split-line" />
            <div className="invoice-row total-row-highlight">
              <span>Grand Total:</span>
              <span>₹{orderData.totalAmount}</span>
            </div>
          </div>
        </div>

      </div>

      {/* Main Items Display Segment Card wrapper */}
      <div className="order-history-card-wrapper">
        <div className="order-card-panel-header single-order-view-header">
          <span className="items-count-indicator">
            Shipment Items ({orderData.items?.length || 0})
          </span>
        </div>
        
        <div className="order-card-panel-body">
          <div className="order-items-inner-collection">
            {orderData.items && orderData.items.map((item, index) => (
              <div key={`${orderData.id}-detail-item-${index}`} className="order-item-row-layout">
                <img 
                  src={item.imageUrl || "https://via.placeholder.com/100?text=No+Image"} 
                  alt={item.productName} 
                  className="order-item-thumbnail-pic" 
                  onClick={function(){navigate(`../Product/${item.productId}`)}}
                />
                <div className="order-item-core-details">
                  <Link to={`/product/${item.productId}`} className="order-item-title-anchor">
                    {item.productName}
                  </Link>
                  <p className="order-item-meta-pricing-specs">
                    Quantity: <span className="dark-bold">{item.quantity}</span> 
                    <span className="divider-spacer">|</span> 
                    Unit Price: <span className="dark-bold">₹{item.unitPrice}</span>
                  </p>
                  <div className="order-item-action-links-row">
                    <Link to={`/product/${item.productId}`} className="continue-shopping-pill-btn special-buy-again-size">
                      Buy it again
                    </Link>
                  </div>
                </div>
                <div className="order-item-right-pricing-block">
                  <span className="item-calculated-subtotal">₹{item.unitPrice * item.quantity}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Order;