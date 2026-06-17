import React, { useContext } from 'react';
import CartContext from '../context/CartContext';
import { useNavigate } from 'react-router-dom';
import api from '../api/axiosConfig'; // Imported to handle silent backend updates for decrementing

const ProductBox = ({ item, isItemBusy, handleAddToCart, handleBuyNow, baseUrl }) => {
  const imageSource = item.imageUrl || 'https://placehold.co/300?text=No+Image';
  const navigate = useNavigate();
  const displaySubCategory = item.subCategory && item.subCategory.includes('_')
    ? item.subCategory.split('_')[1]
    : item.subCategory;

  // Handling cart context
  const { cart: cartData, setCart } = useContext(CartContext);
  const { cart: itemsArray } = cartData;
     
  // 🎯 Find if the current item is already present in the cart array
  const existingCartItem = itemsArray?.find(cartItem => 
    cartItem.productId === item.id || (cartItem.product && cartItem.product.id === item.id)
  );

  // 🔄 Handle Decrementing Item Quantity
  const handleRemoveOneOrDecrement = async () => {
    if (!existingCartItem) return;

    // Create a shallow copy of the items array to modify safely
    let updatedItemsArray = [...itemsArray];
    const itemIndex = updatedItemsArray.findIndex(cartItem => 
      cartItem.productId === item.id || (cartItem.product && cartItem.product.id === item.id)
    );

    if (existingCartItem.quantity > 1) {
      // Reduce local memory count by 1
      updatedItemsArray[itemIndex].quantity -= 1;
    } else {
      // If quantity is 1, remove the object entirely from the list
      updatedItemsArray.splice(itemIndex, 1);
    }

    // Sync state locally immediately for responsive UI snapping
    setCart(prev => ({ ...prev, cart: updatedItemsArray }));

    // Optional: If you have a logged-in user session, update backend server pipeline silently
    const savedUser = localStorage.getItem('user') || null; // standard key or read from UserContext if passed down
    if (savedUser) {
      try {
        const payload = {
          productId: item.id,
          quantity: existingCartItem.quantity > 1 ? existingCartItem.quantity - 1 : 0
        };
        await api.post('/v1/Cart/UpdateCart', payload);
      } catch (err) {
        console.error('Failed syncing decrement subtraction step with server:', err);
      }
    } else {
      // Guest cart fallback persist state
      localStorage.setItem('guest_cart', JSON.stringify(updatedItemsArray));
    }
  };

  return (
    <div className="search-result-row-card">
      
      {/* Left frame: Image Viewport */}
      <div className="row-card-image-viewport">
        <img
          src={imageSource}
          alt={item.name}
          onError={(e) => {
            e.target.onerror = null;
            e.target.src = 'https://placehold.co/300?text=Image+Load+Error';
          }}
          onClick={function(){navigate(`../product/${item.id}`)}}
        />
      </div>

      {/* Right Frame: Specifications Details Panel */}
      <div className="row-card-details-frame">
        <div className="row-card-info-top">
          <h2 className="row-card-headline">{item.name}</h2>

          {/* Ratings Component Mock */}
          <div className="row-card-rating-line">
            <span className="stars-gold">★★★★☆</span>
            <span className="rating-count-link">2,410 ratings</span>
          </div>

          <p className="row-card-category-meta">
            Category: <strong>{item.category}</strong>
            {item.subCategory && (
              <span> | Sub: <strong>{displaySubCategory}</strong></span>
            )}
          </p>

          <p className="row-card-description-body">
            {item.description || 'No product details provided.'}
          </p>

          <p style = {{color:'red'}} className="row-card-discount">
            {item.discount || '0'}% Off
          </p>
        </div>

        {/* Pricing block and actions panel wrapper */}
        <div className="row-card-footer-action-panel">
          <div className="price-tag-container">
            <div className="price-tag-digits">
              <span className="currency-symbol">₹</span>
              <span className="amount-number" style={{marginRight:'9px',textDecorationLine:'line-through'}}>
                {Intl.NumberFormat('en-IN').format(item.catalogPrice)}
              </span>
              <span className="amount-number">
                {item.price}
              </span>
            </div>
            <div className="stock-indicator-height">
              {item.inStock ? (
                <span className="stock-indicator-badge in-stock">
                  In Stock ({item.stock} units)
                </span>
              ) : (
                <span className="stock-indicator-badge out-of-stock">
                  Out of Stock
                </span>
              )}
            </div>
          </div>

          {/* Express Checkout Action Buttons Stack */}
          <div className="row-card-buttons-group">

            {/* 🎯 CONDITIONAL CART CONTROL TOGGLE */}
            {existingCartItem ? (    
                    <div className="cart-quantity-selector-container" style={{display:'flex',alignItems:'center',justifyContent:'space-evenly'}}>
                      <button
                        type="button"
                        onClick={handleRemoveOneOrDecrement}
                        disabled={isItemBusy}
                      >
                        -
                      </button>
                      <span style={{color:'white'}} className="cart-quantity-display-value">{existingCartItem.quantity}</span>
                      <button
                        type="button"
                        onClick={() => handleAddToCart(item, true)}
                        disabled={!item.inStock || isItemBusy || existingCartItem.quantity >= item.stock}
                      >
                        +
                      </button>
                    </div>
            ) : (
              <button
                onClick={() => handleAddToCart(item)}
                disabled={!item.inStock || isItemBusy}
                className={`amazon-pill-btn cart ${!item.inStock ? 'disabled' : ''}`}
              >
                {isItemBusy ? 'Syncing...' : 'Add to Cart'}
              </button>
            )}

            <button
              onClick={() => handleBuyNow(item)}
              disabled={!item.inStock || isItemBusy}
              className={`amazon-pill-btn buy-now ${!item.inStock ? 'disabled' : ''}`}
            >
              Buy Now
            </button>
          </div>
        </div>

      </div>
    </div>
  );
};

export default ProductBox;