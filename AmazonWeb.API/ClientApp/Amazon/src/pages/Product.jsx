import React, { useState, useEffect, useContext } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axiosConfig';
import UserContext from '../context/UserContext';
import { useCart } from '../context/CartContext';
import { baseUrl, checkoutUrl } from '../api/keyUrls';
import ProductRow from '../Components/ProductRow';

// Import assets
import cod from "../assets/cod.png";
import free_shipping from "../assets/icon_free_shipping.png";
import secure_pay from "../assets/secure_pay.png";
import top_brand from "../assets/top_brand.png";


const Product = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const { user } = useContext(UserContext);
  const { cart: cartData, setCart } = useCart();
  const { cart: itemsArray } = cartData;

  const [product, setProduct] = useState(null);
  const [relatedProducts, setRelatedProducts] = useState([]);
  const [uiStatus, setUiStatus] = useState({ loading: true, error: null });
  const [actionLoading, setActionLoading] = useState({});

  const existingCartItem = itemsArray?.find(cartItem =>
    cartItem.productId === id || (cartItem.product && cartItem.product.id === id)
  );

  useEffect(() => {
    const fetchProductDataAndRelated = async () => {
      if (!id) return;
      setUiStatus(prev => ({ ...prev, loading: true }));

      try {
        const productResponse = await api.get(`/v1/Products/${id}`);
        const currentItem = productResponse.data;
        setProduct(currentItem);

        if (currentItem && currentItem.subCategory) {
          const relatedResponse = await api.get(`/v1/Products/subcategory/${encodeURIComponent(currentItem.subCategory)}`);
          const matchingList = Array.isArray(relatedResponse.data) ? relatedResponse.data : [];

          setRelatedProducts(matchingList.filter(item => item.id !== currentItem.id && !item.isDeleted));
        }

        setUiStatus({ loading: false, error: null });
      } catch (err) {
        console.error('Handshake inventory matching execution failure:', err);
        const backendErrorMessage = err.response?.data || err.message || 'Failed to sync product profile details.';
        setUiStatus({
          loading: false,
          error: typeof backendErrorMessage === 'string' ? backendErrorMessage : 'Database service context offline.'
        });
      }
    };

    fetchProductDataAndRelated();
  }, [id]);

  const handleAddToCart = async (targetProduct, silent = false) => {
    const productId = targetProduct.id;
    setActionLoading(prev => ({ ...prev, [productId]: true }));

    let updatedItemsArray = [...itemsArray];
    const existingItemIndex = updatedItemsArray.findIndex(item =>
      item.productId === productId || (item.product && item.product.id === productId)
    );

    if (existingItemIndex !== -1) {
      updatedItemsArray[existingItemIndex].quantity += 1;
    } else {
      updatedItemsArray.push({
        productId: productId,
        quantity: 1,
        name: targetProduct.name,
        price: targetProduct.price,
        imageUrl: targetProduct.imageUrl,
        product: { ...targetProduct }
      });
    }

    try {
      if (user && user.email) {
        const payload = {
          productId: productId,
          quantity: existingItemIndex !== -1 ? updatedItemsArray[existingItemIndex].quantity : 1
        };
        await api.post('/v1/Cart/UpdateCart', payload);
      } else {
        localStorage.setItem('guest_cart', JSON.stringify(updatedItemsArray));
      }

      setCart({ cart: updatedItemsArray, isBusy: false });
      if (!silent) alert(`Successfully added "${targetProduct.name}" to cart!`);
    } catch (err) {
      console.error('Cart assignment error:', err);
      alert(err.response?.data || 'Failed to update shopping cart allocation.');
    } finally {
      setActionLoading(prev => ({ ...prev, [productId]: false }));
    }
  };

  const handleDecrementCart = async (targetProduct) => {
    const productId = targetProduct.id;
    if (!existingCartItem) return;

    setActionLoading(prev => ({ ...prev, [productId]: true }));
    let updatedItemsArray = [...itemsArray];
    const itemIndex = updatedItemsArray.findIndex(item =>
      item.productId === productId || (item.product && item.product.id === productId)
    );

    const nextQty = existingCartItem.quantity - 1;

    if (nextQty > 0) {
      updatedItemsArray[itemIndex].quantity = nextQty;
    } else {
      updatedItemsArray.splice(itemIndex, 1);
    }

    try {
      if (user && user.email) {
        const payload = { productId, quantity: nextQty };
        await api.post('/v1/Cart/UpdateCart', payload);
      } else {
        localStorage.setItem('guest_cart', JSON.stringify(updatedItemsArray));
      }

      setCart({ cart: updatedItemsArray, isBusy: false });
    } catch (err) {
      console.error('Failed syncing decrement operation:', err);
    } finally {
      setActionLoading(prev => ({ ...prev, [productId]: false }));
    }
  };

  const handleBuyNow = (targetProduct) => {
    if (!user || !user.email) {
      alert('Authentication required. Please log in to complete checkout.');
      navigate('/login');
      return;
    }
    navigate(`${checkoutUrl}/${targetProduct.id}`, { state: { directPurchaseItem: targetProduct } });
  };

  if (uiStatus.loading) {
    return (
      <div className="auth-page-container fallback-center">
        <p className="catalog-loading-text">Streaming product specifications matrix from service architecture...</p>
      </div>
    );
  }

  if (uiStatus.error || !product) {
    return (
      <div className="auth-page-container fallback-center">
        <div className="admin-status-alert error alert-constrained">
          <strong>Service Status Intercept:</strong> <br />
          {uiStatus.error || 'Product variant data unreadable.'}
        </div>
      </div>
    );
  }

  const imageSource = product.imageUrl || 'https://placehold.co/400?text=No+Image';
  const displaySubCategory = product.subCategory && product.subCategory.includes('_')
    ? product.subCategory.split('_')[1]
    : product.subCategory;

  return (
    <div className="main-content-fluid product-details-page-override">

      {/* UPPER SECTION: Main Focus Product Split Frame */}
      <div className="product-showcase-container">

        {/* Left Side: Massive Image Viewport Block */}
        <div className="product-image-hero-frame">
          <img
            src={imageSource}
            alt={product.name}
            className="product-hero-img"
            onError={(e) => {
              e.target.onerror = null;
              e.target.src = 'https://placehold.co/400?text=Image+Load+Error';
            }}
          />
        </div>

        {/* Middle Side: Identity, Specs, and Secondary Information */}
        <div className="product-specs-info-panel">
          <h1 className="product-main-title">{product.name}</h1>

          <div className="row-card-rating-line">
            <span className="stars-gold">★★★★☆</span>
            <span className="rating-count-link">2,410 customer reviews</span>
          </div>

          <hr className="product-panel-divider" />

          <p className="product-meta-row">
            Category: <strong className="text-capitalize">{product.category}</strong>
            {product.subCategory && (
              <span> | Subcategory: <strong className="text-capitalize">{displaySubCategory}</strong></span>
            )}
          </p>

          <div className="product-pricing-block">
            <span className="product-price-discount">{product.discount}% <span style={{fontSize:'1.5rem'}}>Off</span></span>
            <br />
            <span className="product-currency-symbol">₹</span>
            <span className="product-price-after">{Intl.NumberFormat('en-IN').format(product.price)}</span>
            <span className="product-price-before">{Intl.NumberFormat('en-IN').format(product.catalogPrice)}</span>

            <div className="product-stock-wrapper">
              {product.inStock ? (
                <span className="stock-indicator-badge in-stock">In Stock ({product.stock} items left)</span>
              ) : (
                <span className="stock-indicator-badge out-of-stock">Temporarily Out of Stock</span>
              )}
            </div>
          </div>

          <div className="product-description-container">
            <h4>Product Information</h4>
            <p className="product-desc-body">{product.description || 'Detailed technical specs haven\'t been allocated for this model option.'}</p>
          </div>
        </div>

        {/* Right Side: Action Buy Panel Options Card */}
        <div className="products-specs-purchase-panel">
          <div className="checkout-delivery-promise-block">
            <span className="delivery-highlight-date">
              FREE delivery <span className="bold-text">Wednesday, June 17</span>
            </span>
            <span className="delivery-subtext">
              Or fastest delivery <span className="bold-text">Sunday, June 14</span>
              <br />
              Order within <span className="timer-green">14 hrs 32 mins</span>
            </span>
          </div>

          <div className="checkout-geo-location-anchor">
            <span className="geo-pin-icon">📍</span>
            <span className="geo-location-text">Deliver to India</span>
          </div>

          <div className="checkout-trust-meta-table">
            <div className="meta-table-row">
              <span className="meta-label">Ships from</span>
              <span className="meta-value link-style">Amazon.com</span>
            </div>
            <div className="meta-table-row">
              <span className="meta-label">Sold by</span>
              <span className="meta-value link-style">RetailerNet Ltd</span>
            </div>
          </div>

          <div className="checkout-gift-checkbox-row">
            <input type="checkbox" id="isAGift" name="isAGift" />
            <label htmlFor="isAGift">Add a gift receipt for easy returns</label>
          </div>

          <div className="product-amazon-trust-badge-row">
            <img className="product-promise-badge-icon" src={secure_pay} alt="secure-pay" />
            <img className="product-promise-badge-icon" src={free_shipping} alt="free_shipping" />
            <img className="product-promise-badge-icon" src={cod} alt="cod_icon" />
            <img className="product-promise-badge-icon" src={top_brand} alt="top_brand" />
          </div>

          <div className="product-express-checkout-row">
            {existingCartItem ? (
              <div className="qty-pill-container">
                <button
                  onClick={() => handleDecrementCart(product)}
                  disabled={actionLoading[product.id]}
                  className="qty-pill-action-btn"
                >
                  −
                </button>
                <span className="qty-pill-display-count">{existingCartItem.quantity}</span>
                <button
                  onClick={() => handleAddToCart(product, true)}
                  disabled={!product.inStock || actionLoading[product.id] || existingCartItem.quantity >= product.stock}
                  className="qty-pill-action-btn"
                >
                  +
                </button>
              </div>
            ) : (
              <button
                onClick={() => handleAddToCart(product)}
                disabled={!product.inStock || actionLoading[product.id]}
                className={`amazon-pill-btn cart ${!product.inStock ? 'disabled' : ''}`}
              >
                {actionLoading[product.id] ? 'Updating...' : 'Add to Cart'}
              </button>
            )}

            <button
              onClick={() => handleBuyNow(product)}
              disabled={!product.inStock || actionLoading[product.id]}
              className={`amazon-pill-btn buy-now ${!product.inStock ? 'disabled' : ''}`}
            >
              Buy Now
            </button>
          </div>
        </div>

      </div>

      {/* LOWER SECTION: Same SubCategory Recommendations Carousel Track */}
      <ProductRow row={relatedProducts} />

    </div>
  );
};

export default Product;