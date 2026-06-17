import React, { useState } from 'react';
import { useCart } from '../context/CartContext';
import { Link, useNavigate } from 'react-router-dom';
import api from '../api/axiosConfig'; 
import { baseUrl, checkoutUrl } from '../api/keyUrls'; // 🎯 FIXED: Cleaned up duplicate inline declarations and grouped imports

const Cart = () => {
  const { cart: cartState, setCart } = useCart();
  const { cart: itemsArray, isBusy } = cartState;

  const [order, setOrder] = useState({ items: [], ShippingAddress: null, PostalCode: null, City: null, Country: null });

  const navigate = useNavigate();

  // Compute pricing summaries safely
  const totalItemsCount = itemsArray?.reduce((sum, item) => sum + (item.quantity || 0), 0) || 0;
  const totalCartPrice = itemsArray?.reduce((sum, item) => sum + ((item.price || 0) * (item.quantity || 0)), 0) || 0;

  /* ==========================================================================
     ⚙️ QUANTITY UPDATER MATRIX
     ========================================================================== */
  const handleQuantityChange = async (productId, currentQuantity, newQuantity) => {
    if (newQuantity < 1) return;

    const originalItemsArray = [...itemsArray];
    const updatedLocalItems = itemsArray.map(item =>
      item.productId === productId ? { ...item, quantity: newQuantity } : item
    );

    setCart({ cart: updatedLocalItems, isBusy: false });

    const token = localStorage.getItem('token');
    if (token) {
      try {
        await api.post('/v1/Cart/UpdateCart', {
          productId: productId,
          quantity: newQuantity
        });
      } catch (err) {
        console.error("Backend quantity sync failed. Rolling back changes:", err);
        setCart({ cart: originalItemsArray, isBusy: false });
      }
    } else {
      localStorage.setItem('guest_cart', JSON.stringify(updatedLocalItems));
    }
  };

  /* ==========================================================================
     🗑️ ITEM REMOVAL ROUTINE
     ========================================================================== */
  const handleRemoveItem = async (productId) => {
    const originalItemsArray = [...itemsArray];
    const updatedLocalItems = itemsArray.filter(item => item.productId !== productId);

    setCart({ cart: updatedLocalItems, isBusy: false });

    const token = localStorage.getItem('token');
    if (token) {
      try {
        await api.delete(`/v1/Cart/RemoveItem?productId=${productId}`);
      } catch (err) {
        console.error("Backend removal failed. Restoring cart context:", err);
        setCart({ cart: originalItemsArray, isBusy: false });
      }
    } else {
      localStorage.setItem('guest_cart', JSON.stringify(updatedLocalItems));
    }
  };

  /* ==========================================================================
     🚀 SECURE ROUTING DISPATCHER
     ========================================================================== */
  const handleCheckoutNavigation = () => {
    const token = localStorage.getItem('token');
    
    if (!token) {
      console.log("No authorization token detected. Redirecting to authentication pathway...");
      // 🎯 FIXED: Added crucial return statement to prevent subsequent lines from executing
      navigate('/login?redirect=checkoutdemo');
      return; 
    }

    // Map properties out to match backend entity schema expectations precisely
    const formattedOrderItems = itemsArray.map(item => ({
      productId: item.productId,
      productName: item.name || item.productName || '',
      imageUrl: item.imageUrl || '',
      quantity: item.quantity || 0,
      unitPrice: item.price || item.unitPrice || 0
    }));

    const orderDataPayload = {
      items: formattedOrderItems,
      totalAmount: totalCartPrice,
      shippingAddress: "", 
      postalCode: "",
      city: "",
      country: ""
    };

    console.log(`Navigating securely to environment route: ${checkoutUrl}`);
    
    // 🎯 FIXED: Passed 'checkoutUrl' directly as a string parameter without wrapping it in an object literal {}
    navigate(checkoutUrl, { state: { orderData: orderDataPayload } });
  };

  if (isBusy) {
    return (
      <div className="cart-loading-spinner-box">
        <p>Loading your shopping basket details...</p>
      </div>
    );
  }

  return (
    <div className="cart-page-fluid-container">
      <div className="cart-main-layout-wrapper">

        {/* LEFT COLUMN: SHOPPING CART ITEM ROW */}
        <div className="cart-items-collection-panel">
          <div className="cart-header-title-block">
            <h1>Shopping Cart</h1>
            {itemsArray && itemsArray.length > 0 && <span className="cart-price-header-label">Price</span>}
          </div>
          <hr className="cart-layout-divider" />

          {(!itemsArray || itemsArray.length === 0) ? (
            <div className="cart-empty-state-fallback">
              <h3>Your Shopping Cart is empty.</h3>
              <p>Check out today's deals or continue exploring our product catalog.</p>
              <Link to="/" className="amazon-primary-btn style-inline">Continue Shopping</Link>
            </div>
          ) : (
            itemsArray.map((item) => (
              <div key={item.productId} className="cart-item-row-node">
                <div className="cart-item-image-wrapper">
                  <img src={item.imageUrl} alt={item.name || 'Catalog Product'} onClick={function(){navigate(`../product/${item.productId}`)}} />
                </div>

                <div className="cart-item-details-body">
                  <h2 className="cart-item-title-text">{item.name || "Amazon Verified Product"}</h2>
                  <p className="cart-item-stock-status">In Stock</p>
                  <p className="cart-item-shipping-promo">Eligible for FREE Shipping</p>

                  <div className="cart-item-actions-row">
                    <div className="cart-quantity-selector-container">
                      <button
                        type="button"
                        onClick={() => handleQuantityChange(item.productId, item.quantity, item.quantity - 1)}
                        disabled={item.quantity <= 1}
                      >
                        -
                      </button>
                      <span className="cart-quantity-display-value">{item.quantity}</span>
                      <button
                        type="button"
                        onClick={() => handleQuantityChange(item.productId, item.quantity, item.quantity + 1)}
                      >
                        +
                      </button>
                    </div>
                    <span className="cart-action-split-pipe">|</span>
                    <button
                      type="button"
                      className="cart-delete-trigger-btn"
                      onClick={() => handleRemoveItem(item.productId)}
                    >
                      Delete
                    </button>
                  </div>
                </div>

                <div className="cart-item-price-column">
                  <span className="cart-item-calculated-price">
                    ₹{(item.price || 0).toLocaleString('en-IN')}
                  </span>
                </div>
              </div>
            ))
          )}

          {itemsArray && itemsArray.length > 0 && (
            <div className="cart-subtotal-summary-row border-top-split">
              <h3>Subtotal ({totalItemsCount} item{totalItemsCount !== 1 ? 's' : ''}): <strong>₹{totalCartPrice.toLocaleString('en-IN')}</strong></h3>
            </div>
          )}
        </div>

        {/* RIGHT COLUMN: STICKY ACCESSIBILITY PANEL */}
        {itemsArray && itemsArray.length > 0 && (
          <div className="cart-checkout-sticky-panel">
            <div className="checkout-widget-block boundary-bottom-split">
              <div className="checkout-subtotal-preview">
                <h2>Subtotal ({totalItemsCount} items): <br /><strong>₹{totalCartPrice.toLocaleString('en-IN')}</strong></h2>
              </div>
              <div className="checkout-free-shipping-indicator">
                <span className="checkmark-icon">✓</span> Your order qualifies for FREE Delivery.
              </div>
              <button
                type="button"
                className="amazon-primary-btn checkout-action-btn-w100"
                onClick={handleCheckoutNavigation}
              >
                Proceed to Checkout
              </button>
            </div>
          </div>
        )}

      </div>
    </div>
  );
};

export default Cart;