import React, { useState, useEffect, useContext } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import api from '../api/axiosConfig';
import UserContext from '../context/UserContext'; 
import { useCart } from '../context/CartContext'; 
import { baseUrl, checkoutUrl } from '../api/keyUrls';
import ProductBox from '../Components/ProductBox';
import WarningDialog from '../Components/WarningDialog';

const SearchResult = () => {
  const { user } = useContext(UserContext); 
  const { cart, setCart } = useCart(); 
  const navigate = useNavigate();
  const location = useLocation();
  
  // 🎛️ Warning Dialog Overlay States
  const [dialog, setDialog] = useState({ isOpen: false, message: '' });
  
  // Helper trigger to handle open dialogue shifts
  const triggerWarning = (msg) => {
    setDialog({ isOpen: true, message: msg });
  };

  const [products, setProducts] = useState([]);
  const [uiStatus, setUiStatus] = useState({ loading: true, error: null });
  const [actionLoading, setActionLoading] = useState({}); 

  // 🎛️ Filter and Sort States (Bound to backend API filters)
  const [sortBy, setSortBy] = useState('');
  const [maxPrice, setMaxPrice] = useState('');
  const [minRating, setMinRating] = useState('');

  // 🎯 Extract 'q' and 'category' from the browser URL parameter location matrix
  const searchParams = new URLSearchParams(location.search);
  const qParam = searchParams.get('q') || '';
  const categoryParam = searchParams.get('category') || '';

  // 📡 Fetch searched products by dynamically matching your ASP.NET route endpoints
  useEffect(() => {
    const fetchSearchResults = async () => {
      setUiStatus(prev => ({ ...prev, loading: true }));
      try {
        let endpoint = '/v1/Products';
        const params = {};

        // 1. Map to side filters if selected
        if (sortBy) params.sortBy = sortBy;
        if (maxPrice) params.maxPrice = maxPrice;
        if (minRating) params.minRating = minRating;

        // 2. Route Routing Strategy: Chooses the specific route defined in your C# Controller
        if (qParam.trim()) {
          // Hits: [HttpGet("search/{name}")]
          endpoint = `/v1/Products/search/${encodeURIComponent(qParam.trim())}`;
        } else if (categoryParam && categoryParam !== 'All') {
          // If the string contains an underscore, it's a SubCategory enum item
          if (categoryParam.includes('_')) {
            // Hits: [HttpGet("subcategory/{subCategory}")]
            endpoint = `/v1/Products/subcategory/${encodeURIComponent(categoryParam)}`;
          } else {
            // Hits: [HttpGet("category/{category}")]
            endpoint = `/v1/Products/category/${encodeURIComponent(categoryParam)}`;
          }
        }

        // Send request to the selected .NET backend endpoint path
        const response = await api.get(endpoint, { params });
        const catalogData = Array.isArray(response.data) ? response.data : [];
        
        setProducts(catalogData);
        setUiStatus({ loading: false, error: null });
      } catch (err) {
        console.error('Handshake search result catalog error context:', err);
        const backendErrorMessage = err.response?.data || err.message || 'Failed to sync product inventory.';
        setUiStatus({
          loading: false,
          error: typeof backendErrorMessage === 'string' ? backendErrorMessage : 'Database search service offline.'
        });
      }
    };

    fetchSearchResults();
  }, [qParam, categoryParam, sortBy, maxPrice, minRating]);

  // 🛒 Handle Add to Cart Strategy
  const handleAddToCart = async (product, silent = false) => {
    const productId = product.id;
    setActionLoading(prev => ({ ...prev, [productId]: true }));

    let updatedItemsArray = [...cart.cart];
    
    const existingItemIndex = updatedItemsArray.findIndex(item => 
      item.productId === productId || (item.product && item.product.id === productId)
    );

    if (existingItemIndex !== -1) {
      updatedItemsArray[existingItemIndex].quantity += 1;
    } else {
      updatedItemsArray.push({
        productId: product.id,
        quantity: 1,
        name: product.name,
        price: product.price,
        imageUrl: product.imageUrl,
        product: {
          id: product.id,
          name: product.name,
          price: product.price,
          imageUrl: product.imageUrl
        }
      });
    }

    if (user && user.email) {
      try {
        const payload = {
          productId: productId,
          quantity: existingItemIndex !== -1 ? updatedItemsArray[existingItemIndex].quantity : 1
        };

        await api.post('/v1/Cart/UpdateCart', payload);
        setCart({ cart: updatedItemsArray, isBusy: false });

        if (!silent) {
          console.log(`Successfully added "${product.name}" to your account cart!`);
        }
      } catch (err) {
        console.error('Cart operation failure context:', err);
        triggerWarning(err.response?.data || 'Failed to update shopping cart allocation.');
      } finally {
        setActionLoading(prev => ({ ...prev, [productId]: false }));
      }
    } else {
      try {
        localStorage.setItem('guest_cart', JSON.stringify(updatedItemsArray));
        setCart({ cart: updatedItemsArray, isBusy: false });

        if (!silent) {
          console.log(`"${product.name}" added to guest cart!`);
        }
      } catch (err) {
        console.error('Local storage cart operation exception context:', err);
        triggerWarning('Failed to update local guest cart matrix space.');
      } finally {
        setActionLoading(prev => ({ ...prev, [productId]: false }));
      }
    }
  };

  const handleBuyNow = (product) => {
    if (!user || !user.email) {
      triggerWarning('Authentication required. Please log in to complete an express purchase.');
      navigate('/login');
      return;
    }
    navigate(`${checkoutUrl}/${product.id}`, { state: { directPurchaseItem: product } });
  };

  const clearFilters = () => {
    setSortBy('');
    setMaxPrice('');
    setMinRating('');
  };

  if (uiStatus.loading && products.length === 0) {
    return (
      <div className="auth-page-container fallback-center">
        <p className="catalog-loading-text">Streaming matching search items from backend database service layer...</p>
      </div>
    );
  }

  if (uiStatus.error) {
    return (
      <div className="auth-page-container fallback-center">
        <div className="admin-status-alert error alert-constrained">
          <strong>Search Engine Intercept Error:</strong> <br />
          {uiStatus.error}
        </div>
      </div>
    );
  }

  return (
    <div className="main-content-fluid catalog-root-override">
      
      {/* Top Banner Meta Info Bar */}
      <div className="catalog-meta-banner">
        <p>
          Showing {products.length} matching results 
          {/* 🛠️ UPDATED META TEXT: If text input exists, display it. Only show the category suffix if q is absent */}
          {qParam ? (
            <span> for "{qParam}"</span>
          ) : (
            categoryParam && categoryParam !== 'All' && (
              <span> in category "{categoryParam.includes('_') ? categoryParam.split('_')[1] : categoryParam}"</span>
            )
          )}
        </p>
      </div>

      {/* Main Layout Scaffolding */}
      <div className="catalog-scaffolding">
        
        {/* 🛠️ LEFT SIDEBAR FILTERS */}
        <aside className="catalog-filter-sidebar">
          <div className="sidebar-filter-header">
            <h3>Filters</h3>
            <button onClick={clearFilters} className="clear-filters-btn">Clear all</button>
          </div>

          <div className="sidebar-filter-groups-wrapper">
            {/* Sort Block */}
            <div className="filter-group-block">
              <h4>Sort By</h4>
              <select value={sortBy} onChange={(e) => setSortBy(e.target.value)} className="sidebar-select-input">
                <option value="">Featured</option>
                <option value="price_asc">Price: Low to High</option>
                <option value="price_desc">Price: High to Low</option>
                <option value="rating_desc">Avg. Customer Review</option>
              </select>
            </div>

            {/* Price Radio Block */}
            <div className="filter-group-block">
              <h4>Price Budget</h4>
              <div className="filter-radio-stack">
                <label>
                  <input type="radio" name="price" checked={maxPrice === ''} onChange={() => setMaxPrice('')} /> Any Price
                </label>
                <label>
                  <input type="radio" name="price" checked={maxPrice === '1000'} onChange={() => setMaxPrice('1000')} /> Under ₹1,000
                </label>
                <label>
                  <input type="radio" name="price" checked={maxPrice === '5000'} onChange={() => setMaxPrice('5000')} /> Under ₹5,000
                </label>
                <label>
                  <input type="radio" name="price" checked={maxPrice === '10000'} onChange={() => setMaxPrice('10000')} /> Under ₹10,000
                </label>
              </div>
            </div>

            {/* Star Rating Block */}
            <div className="filter-group-block">
              <h4>Customer Review</h4>
              <div className="filter-radio-stack">
                {[4, 3, 2, 1].map((stars) => (
                  <button
                    key={stars}
                    onClick={() => setMinRating(stars.toString())}
                    className={`rating-star-filter-link ${minRating === stars.toString() ? 'active' : ''}`}
                  >
                    {'★'.repeat(stars)}{'☆'.repeat(5 - stars)} & Up
                  </button>
                ))}
              </div>
            </div>
          </div>
        </aside>

        {/* 📋 RIGHT CONTENT FIELD: Filtered Product Results Column Grid */}
        <main className="catalog-results-viewspace">
          {products.length === 0 && (
            <div className="catalog-empty-container">
              <h3>No matching products found</h3>
              <p>We couldn't find anything matching your search criteria. Try modifying your keywords or adjusting sidebar parameters.</p>
            </div>
          )}

          <div className="catalog-vertical-stack">
            {products
              .filter(item => !item.isDeleted)
              .map((item) => {

                return (
                  // 🎯 RENDER THE NEW COMPONENT INSTANCE HERE USING PROPS MAPPING
                <ProductBox
                  key={item.id}
                  item={item}
                  isItemBusy={actionLoading[item.id] || false}
                  handleAddToCart={handleAddToCart}
                  handleBuyNow={handleBuyNow}
                  baseUrl={baseUrl}
                />
                );
              })}
          </div>
        </main>
      </div>
    </div>
  );
};

export default SearchResult;