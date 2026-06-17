import React, { useEffect, useState, useContext } from 'react';
import axios from 'axios';
import { Link, useNavigate } from 'react-router-dom';
import api from '../api/axiosConfig';
import UserContext from '../context/UserContext';

const Orders = () => {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();
  
    //get user
    const {user} = useContext(UserContext)

    //send to login if not logged in 
    if(!user){
      navigate('/login')
    }

  useEffect(() => {
    const fetchUserOrders = async () => {
      try {
        setLoading(true);
        // Assuming you have configured axios defaults or interceptors to attach your JWT bearer token
        const response = await api.get('v1/Orders/GetOrderByUserID');
        console.log(response)
        // Ensure data maps cleanly to an array fallback
        setOrders(response.data || []);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching historic order summaries:", err);
        setError(err.response?.data || "Unable to retrieve order history at this time.");
        setLoading(false);
      }
    };

    fetchUserOrders();
  }, []);

  // Helper function to render status badges matching your OrderStatus options
  const getStatusBadgeClass = (status) => {
    switch (status?.toLowerCase()) {
      case 'processing': return 'status-badge-blue';
      case 'shipped': return 'status-badge-amber';
      case 'delivered': return 'status-badge-green';
      case 'cancelled':
      case 'failed': return 'status-badge-red';
      default: return 'status-badge-gray';
    }
  };

  // Helper function to format C# ISO string timestamps into pristine local date values
  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString(undefined, options);
  };

  if (loading) {
    return (
      <div className="orders-loading-spinner-wrapper">
        <div className="amazon-loading-shimmer-circle"></div>
        <p>Loading your order history...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="orders-error-panel-boundary">
        <div className="error-alert-box">
          <h5>An error occurred</h5>
          <p>{error}</p>
        </div>
      </div>
    );
  }

  //direct to individual Order
  const toOrder = (order)=>{

    console.log(order)

    const orderPayload = {
        id:order.id,
        userId: order.userId,
        ordeDate: order.orderDate,
        city: order.city,
        country: order.country,
        postalCode: order.postalCode,
        shippingAddress: order.shippingAddress,
        totalAmount: order.totalAmount,
        orderStatus: order.status,
        items: order.items
    }

    //go to that page
    navigate('/Order',{state:{orderData: orderPayload}});
  }

  return (
    <div className="orders-dashboard-fluid-container">
      <nav className="orders-breadcrumb-trail">
        <Link to="/account">Your Account</Link> › <span className="active-trail">Your Orders</span>
      </nav>

      <h1 className="orders-page-main-heading">Your Orders</h1>

      {orders.length === 0 ? (
        <div className="orders-empty-history-card">
          <p>You haven't placed any orders yet.</p>
          <Link to="/" className="continue-shopping-pill-btn">Continue Shopping</Link>
        </div>
      ) : (
        <div className="orders-history-list-stream">
          {orders.map((order) => (
            <div key={order.id} className="order-history-card-wrapper" onClick={function(){toOrder(order)}}>
              
              {/* Card Meta-Header Bar */}
              <div className="order-card-panel-header">
                <div className="header-meta-left-group">
                  <div className="meta-info-block">
                    <span className="meta-label-text">ORDER PLACED</span>
                    <span className="meta-value-text">{formatDate(order.orderDate)}</span>
                  </div>
                  <div className="meta-info-block">
                    <span className="meta-label-text">TOTAL</span>
                    <span className="meta-value-text highlight-price">₹{order.totalAmount}</span>
                  </div>
                  <div className="meta-info-block ship-to-block-hide">
                    <span className="meta-label-text">SHIP TO</span>
                    <span className="meta-value-text target-address-link" title={`${order.shippingAddress}, ${order.city}`}>
                      {order.city || "Delivery Address"}
                    </span>
                  </div>
                </div>
                <div className="header-meta-right-group">
                  <span className="meta-label-text">ORDER # {order.id}</span>
                </div>
              </div>

              {/* Card Body Display */}
              <div className="order-card-panel-body">
                
                {/* Delivery Status Row */}
                <div className="order-status-timeline-strip">
                  <span className={`status-pill-indicator ${getStatusBadgeClass(order.status)}`}>
                    {order.status}
                  </span>
                </div>

                {/* Grid Stream of Items Inside this Particular Order */}
                <div className="order-items-inner-collection">
                  {order.items && order.items.map((item, index) => (
                    <div key={`${order.id}-item-${index}`} className="order-item-row-layout">
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
                          Quantity: <span className="dark-bold">{item.quantity}</span> | Price: <span className="dark-bold">₹{item.unitPrice}</span>
                        </p>
                        <div className="order-item-action-links-row">
                          <Link to={`/product/${item.productId}`} className="action-btn-pill-small">Buy it again</Link>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>

              </div>

            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Orders;