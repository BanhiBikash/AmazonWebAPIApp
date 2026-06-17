import React, { useState, useEffect } from 'react';
import api from '../api/axiosConfig';
import CategorySubCategory from '../context/CategorySubCategory';
import { useContext } from 'react';

const ProductAdd = () => {
  // --- 🎯 Base States for Product Addition & Layout Metadata ---
  const [productData, setProductData] = useState({
    name: '',
    price: '',
    stock: '',
    description: '',
    category: '',
    subCategory: ''
  });

  const [filteredSubCategories, setFilteredSubCategories] = useState([]);
  const [thumbnailFile, setThumbnailFile] = useState(null);
  const {category, setCategory} = useContext(CategorySubCategory)
  const {categoryArray, subCategoryArray} = category;
  const [uiStatus, setUiStatus] = useState({
    loading: false,
    fetchLoading: true,
    success: null,
    error: null,
    searchLoading: false
  });

  // --- 🔍 Search Management States ---
  const [searchQuery, setSearchQuery] = useState('');
  const [searchFilterType, setSearchFilterType] = useState('name'); // 'name' | 'id' | 'category' | 'subcategory'
  const [searchResults, setSearchResults] = useState([]);

  // --- 🛠️ Edit Modal States ---
  const [editingProduct, setEditingProduct] = useState(null); 
  const [editForm, setEditForm] = useState({
    id: '', 
    name: '', 
    price: '', 
    discount: '', // 👈 Added discount field 
    stock: '', 
    description: '', 
    imageUrl: ''
  });
  const [editThumbnail, setEditThumbnail] = useState(null);

  // Dynamic Prefix Translation Dictionary linking your C# Enums
  const prefixMap = {
    'Mobiles': 'Mobile_', 'Laptops': 'Laptop_', 'Fashion': 'Fashion_', 'Books': 'Book_',
    'HomeAppliances': 'HomeAppliance_', 'Furniture': 'Furniture_', 'Toys': 'Toy_',
    'Sports': 'Sports_', 'Beauty': 'Beauty_', 'Health': 'Health_', 'Groceries': 'Grocery_',
    'Pets': 'Pet_', 'Automotive': 'Automotive_', 'Jewelry': 'Jewelry_', 'Shoes': 'Shoe_',
    'Stationary': 'Stationary_', 'Common': 'Common'
  };

  // 📡 Fetch enum lookups and load all products on mount
  useEffect(() => {
    const fetchMetadataAndProducts = async () => {
      try {
        const response = await api.get('v1/Products/GetCategories');
        const { categories, subCategories } = response.data;

        setCategory({categoryArray:categories, subCategoryArray:subCategories});

        if (categoryArray.length > 0) {
          const firstCatId = categoryArray[0].id.toString();
          setProductData(prev => ({ ...prev, category: firstCatId }));
          filterSubCategoriesList(firstCatId, categoryArray, subCategoryArray);
        }

        // Hydrate initial inventory tracking view
        const prodResponse = await api.get('v1/Products');
        setSearchResults(prodResponse.data || []);

        setUiStatus(prev => ({ ...prev, fetchLoading: false }));
      } catch (err) {
        console.error('Failed fetching product startup payload:', err);
        setUiStatus(prev => ({
          ...prev,
          fetchLoading: false,
          error: 'Failed to synchronize layout mappings from server.'
        }));
      }
    };

    fetchMetadataAndProducts();
  }, []);

  const filterSubCategoriesList = (categoryIdStr, currentCats, currentSubs) => {
    const numericId = parseInt(categoryIdStr, 10);
    const selectedCategoryName = currentCats.find(c => c.id === numericId)?.name;

    if (!selectedCategoryName) {
      setFilteredSubCategories([]);
      setProductData(prev => ({ ...prev, category: categoryIdStr, subCategory: '' }));
      return;
    }

    const matchPrefix = prefixMap[selectedCategoryName] || `${selectedCategoryName}_`;
    const filtered = currentSubs.filter(sub => sub.name.startsWith(matchPrefix));

    setFilteredSubCategories(filtered);
    setProductData(prev => ({ ...prev, category: categoryIdStr, subCategory: '' }));
  };

  const handleCategoryChange = (e) => {
    const targetCategoryId = e.target.value;
    filterSubCategoriesList(targetCategoryId, categoryArray, subCategoryArray);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setProductData({ ...productData, [name]: value });
  };

  const handleFileChange = (e) => {
    if (e.target.files && e.target.files[0]) {
      setThumbnailFile(e.target.files[0]);
    }
  };

  // --- ➕ Add Product Form Submission Handler ---
  const handleFormSubmit = async (e) => {
    e.preventDefault();
    if (!productData.category) {
      setUiStatus(prev => ({ ...prev, error: 'Please select a valid primary category.' }));
      return;
    }

    setUiStatus(prev => ({ ...prev, loading: true, success: null, error: null }));

    const multiPartForm = new FormData();
    const stockCount = parseInt(productData.stock, 10) || 0;

    multiPartForm.append('Name', productData.name);
    multiPartForm.append('Price', parseInt(productData.price, 10) || 0);
    multiPartForm.append('Stock', stockCount);
    multiPartForm.append('InStock', stockCount > 0);
    multiPartForm.append('Description', productData.description);
    multiPartForm.append('Category', parseInt(productData.category, 10));

    if (productData.subCategory && productData.subCategory !== '') {
      multiPartForm.append('SubCategory', parseInt(productData.subCategory, 10));
    }
    if (thumbnailFile) {
      multiPartForm.append('Thumbnail', thumbnailFile);
    }

    try {
      await api.post('/v1/Products', multiPartForm);
      setUiStatus(prev => ({ ...prev, loading: false, success: 'Product successfully saved to database catalog!', error: null }));

      setProductData(prev => ({ ...prev, name: '', price: '', stock: '', description: '', subCategory: '' }));
      setThumbnailFile(null);
      if (document.getElementById('thumbnail')) document.getElementById('thumbnail').value = '';

      refreshCatalogGrid();
    } catch (err) {
      console.error(err);
      const backendErrorMessage = err.response?.data?.message || err.message || 'Server data injection failure.';
      setUiStatus(prev => ({ ...prev, loading: false, success: null, error: backendErrorMessage }));
    }
  };

  // --- 🔍 Universal Multi-Filter Dispatch Handler ---
  const handleCatalogSearch = async (e) => {
    e.preventDefault();
    setUiStatus(prev => ({ ...prev, searchLoading: true, error: null }));

    try {
      let endpoint = 'v1/Products';
      const cleanQuery = searchQuery.trim();

      if (cleanQuery !== '') {
        if (searchFilterType === 'name') {
          endpoint = `v1/Products/search/${encodeURIComponent(cleanQuery)}`;
        } else if (searchFilterType === 'id') {
          endpoint = `v1/Products/${cleanQuery}`;
        } else if (searchFilterType === 'category') {
          endpoint = `v1/Products/category/${encodeURIComponent(cleanQuery)}`;
        } else if (searchFilterType === 'subcategory') {
          endpoint = `v1/Products/subcategory/${encodeURIComponent(cleanQuery)}`;
        }
      }

      const response = await api.get(endpoint);
      const dataPayload = Array.isArray(response.data) ? response.data : [response.data];
      setSearchResults(dataPayload.filter(p => p !== null));
    } catch (err) {
      console.error('Search query failure:', err);
      setSearchResults([]);
      setUiStatus(prev => ({
        ...prev,
        error: err.response?.status === 404 ? 'No matching products discovered for the query filter.' : 'Search endpoint evaluation failure.'
      }));
    } finally {
      setUiStatus(prev => ({ ...prev, searchLoading: false }));
    }
  };

  // --- ❌ Delete Product Handler ---
  const handleProductDelete = async (productId) => {
    if (!window.confirm('Are you absolutely certain you want to remove this product row from catalog inventory?')) return;

    setUiStatus(prev => ({ ...prev, error: null, success: null }));
    try {
      await api.delete(`v1/Products/${productId}`);
      setUiStatus(prev => ({ ...prev, success: 'Inventory row successfully removed from database.' }));
      setSearchResults(prev => prev.filter(item => item.id !== productId));
    } catch (err) {
      console.error('Failed to execute delete operations:', err);
      setUiStatus(prev => ({ ...prev, error: err.response?.data || 'Authorization rejection or target offline.' }));
    }
  };

  // --- 📝 Update / Edit Operation Flows ---
  const launchEditContext = (product) => {
    setEditingProduct(product);
    setEditForm({
      id: product.id,
      name: product.name,
      price: product.price,
      discount: product.discount !== undefined && product.discount !== null ? product.discount : 0, // 👈 Capture current discount mapping
      stock: product.stock,
      description: product.description,
      imageUrl: product.imageUrl || ''
    });
    setEditThumbnail(null);
  };

  const handleEditChange = (e) => {
    const { name, value } = e.target;
    setEditForm(prev => ({ ...prev, [name]: value }));
  };

  const handleEditFormSubmit = async (e) => {
    e.preventDefault();
    setUiStatus(prev => ({ ...prev, loading: true, error: null, success: null }));

    const updateFormPayload = new FormData();
    updateFormPayload.append('Id', editForm.id);
    updateFormPayload.append('Name', editForm.name);
    updateFormPayload.append('Price', parseInt(editForm.price, 10) || 0);
    updateFormPayload.append('Discount', parseInt(editForm.discount, 10) || 0); // 👈 Append discount numerical value to payload
    updateFormPayload.append('Stock', parseInt(editForm.stock, 10) || 0);
    updateFormPayload.append('InStock', (parseInt(editForm.stock, 10) || 0) > 0);
    updateFormPayload.append('Description', editForm.description);
    updateFormPayload.append('ImageUrl', editForm.imageUrl);

    if (editThumbnail) {
      updateFormPayload.append('Thumbnail', editThumbnail);
    }

    try {
      await api.put('v1/Products', updateFormPayload);
      setUiStatus(prev => ({ ...prev, loading: false, success: 'Product updates committed successfully.' }));
      setEditingProduct(null);
      refreshCatalogGrid();
    } catch (err) {
      console.error(err);
      setUiStatus(prev => ({ ...prev, loading: false, error: err.response?.data?.message || 'Failed to update catalog.' }));
    }
  };

  const refreshCatalogGrid = async () => {
    try {
      const prodResponse = await api.get('v1/Products');
      setSearchResults(prodResponse.data || []);
    } catch (e) { console.error('Grid reload issue:', e); }
  };

  if (uiStatus.fetchLoading) {
    return (
      <div className="auth-page-container" style={{ justifyContent: 'center' }}>
        <p style={{ fontSize: '0.9rem', color: '#666' }}>Synchronizing Enum structures from ASP.NET API stream...</p>
      </div>
    );
  }

  return (
    <div className="auth-page-container" style={{ gap: '2rem' }}>

      {/* SECTION 1: ADD NEW PRODUCT FORM SHELL */}
      <div className="auth-card-box register-card-wide">
        <h1 className="auth-card-title">Product Management Hub</h1>
        <p style={{ fontSize: '0.8rem', color: '#666', margin: '-10px 0 15px 0' }}>Admin Console: Add catalog inventory rows</p>

        {uiStatus.success && <div className="admin-status-alert success">{uiStatus.success}</div>}
        {uiStatus.error && <div className="admin-status-alert error">{uiStatus.error}</div>}

        <form onSubmit={handleFormSubmit} className="auth-form-flow">
          <div className="auth-input-group">
            <label htmlFor="name">Product Name</label>
            <input
              type="text"
              id="name"
              name="name"
              maxLength={100}
              value={productData.name}
              onChange={handleChange}
              required
            />
          </div>

          <div className="auth-form-row-grid">
            <div className="auth-input-group">
              <label htmlFor="price">Price (INR - Integer)</label>
              <input
                type="number"
                id="price"
                name="price"
                min="0"
                value={productData.price}
                onChange={handleChange}
                required
              />
            </div>
            <div className="auth-input-group">
              <label htmlFor="stock">Stock Quantity</label>
              <input
                type="number"
                id="stock"
                name="stock"
                min="0"
                value={productData.stock}
                onChange={handleChange}
                required
              />
            </div>
          </div>

          <div className="auth-form-row-grid">
            <div className="auth-input-group">
              <label htmlFor="category">Category</label>
              <select
                id="category"
                name="category"
                value={productData.category}
                onChange={handleCategoryChange}
                className="auth-select-field"
                required
              >
                {categoryArray.map(cat => (
                  <option key={cat.id} value={cat.id}>{cat.name}</option>
                ))}
              </select>
            </div>

            <div className="auth-input-group">
              <label htmlFor="subCategory">Sub-Category (Optional)</label>
              <select
                id="subCategory"
                name="subCategory"
                value={productData.subCategory}
                onChange={handleChange}
                className="auth-select-field"
                disabled={filteredSubCategories.length === 0}
              >
                <option value="">None / No Subcategory</option>
                {filteredSubCategories.map(sub => {
                  const displayLabel = sub.name.includes('_') ? sub.name.split('_')[1] : sub.name;
                  return <option key={sub.id} value={sub.id}>{displayLabel}</option>;
                })}
              </select>
            </div>
          </div>

          <div className="auth-input-group">
            <label htmlFor="thumbnail">Product Thumbnail Image File</label>
            <input
              type="file"
              id="thumbnail"
              name="thumbnail"
              accept="image/*"
              onChange={handleFileChange}
              required
            />
          </div>

          <div className="auth-input-group">
            <label htmlFor="description">Product Specification Description</label>
            <textarea
              id="description"
              name="description"
              rows="3"
              className="admin-textarea-field"
              value={productData.description}
              onChange={handleChange}
              required
            />
          </div>

          <button
            type="submit"
            className="auth-action-btn-gold"
            disabled={uiStatus.loading}
            style={{ padding: '8px 0', fontWeight: '700' }}
          >
            {uiStatus.loading ? 'Uploading Data Streams...' : 'Publish Product to Catalog'}
          </button>
        </form>
      </div>

      {/* SECTION 2: SEARCH ENGINE & INVENTORY UTILITY MANAGEMENT LISTING GRID */}
      <div className="auth-card-box register-card-wide" style={{ background: '#fcfcfc' }}>
        <h2 className="auth-card-title" style={{ fontSize: '1.35rem' }}>Catalog Inventory Controller</h2>
        <p style={{ fontSize: '0.8rem', color: '#666', margin: '-10px 0 15px 0' }}>Filter rows real-time to alter or drop live elements</p>

        <form onSubmit={handleCatalogSearch} className="auth-form-flow" style={{ marginBottom: '20px' }}>
          <div className="auth-form-row-grid">
            <div className="auth-input-group">
              <label htmlFor="searchFilterType">Filter Criteria</label>
              <select
                id="searchFilterType"
                value={searchFilterType}
                onChange={(e) => setSearchFilterType(e.target.value)}
                className="auth-select-field"
              >
                <option value="name">Product Name String</option>
                <option value="id">System GUID ID</option>
                <option value="category">Category (Enum Literal)</option>
                <option value="subcategory">Sub-Category (Enum Literal)</option>
              </select>
            </div>
            <div className="auth-input-group">
              <label htmlFor="searchQuery">Search Value</label>
              <input
                type="text"
                id="searchQuery"
                placeholder={searchFilterType === 'id' ? 'e.g., 3fa85f64-5717...' : 'Type filter queries...'}
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
          </div>
          <button type="submit" className="auth-secondary-create-btn" style={{ padding: '6px 0' }}>
            {uiStatus.searchLoading ? 'Scanning Index Matrix...' : 'Filter Catalog Grid'}
          </button>
        </form>

        <div style={{ maxHeight: '400px', overflowY: 'auto', border: '1px solid #e7e7e7', borderRadius: '4px', background: '#fff' }}>
          {searchResults.length === 0 ? (
            <p style={{ textAlign: 'center', padding: '20px', fontSize: '0.85rem', color: '#777' }}>No records residing in filtered scope.</p>
          ) : (
            searchResults.map(item => (
              <div key={item.id} style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '12px', borderBottom: '1px solid #eee' }}>
                <img
                  src={item.imageUrl || 'https://via.placeholder.com/50'}
                  alt=""
                  style={{ width: '45px', height: '45px', objectFit: 'contain', background: '#f9f9f9', borderRadius: '4px' }}
                />
                <div style={{ flex: 1, minWidth: 0 }}>
                  <h4 style={{ margin: '0 0 2px 0', fontSize: '0.88rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', color: '#111' }}>{item.name}</h4>
                  <p style={{ margin: 0, fontSize: '0.75rem', color: '#666' }}>
                    Price: <strong>₹{item.price}</strong> {item.discount > 0 && <span>(Disc: <strong>{item.discount}%</strong>)</span>} | Stock: <strong>{item.stock}</strong> units
                  </p>
                  <p style={{ margin: 0, fontSize: '0.65rem', color: '#999', fontFamily: 'monospace' }}>{item.id}</p>
                </div>
                <div style={{ display: 'flex', gap: '6px' }}>
                  <button
                    onClick={() => launchEditContext(item)}
                    style={{ background: '#f0f2f2', border: '1px solid #adb1b8', padding: '4px 10px', fontSize: '0.75rem', borderRadius: '4px', cursor: 'pointer' }}
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => handleProductDelete(item.id)}
                    style={{ background: '#fff1f1', border: '1px solid #ba0933', color: '#ba0933', padding: '4px 10px', fontSize: '0.75rem', borderRadius: '4px', cursor: 'pointer' }}
                  >
                    Delete
                  </button>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* SECTION 3: CONDITIONAL PRODUCT UPDATE CONTEXT MODAL OVERLAY */}
      {editingProduct && (
        <div style={{ position: 'fixed', top: 0, left: 0, width: '100vw', height: '100vh', background: 'rgba(0,0,0,0.4)', zIndex: 2000, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '20px', boxSizing: 'border-box', overflowY: 'auto' }}>
          <div className="auth-card-box register-card-wide" style={{ margin: 'auto', boxShadow: '0 4px 20px rgba(0,0,0,0.2)' }}>
            <h2 className="auth-card-title" style={{ fontSize: '1.35rem' }}>Modify Catalog Record</h2>
            <p style={{ fontSize: '0.75rem', color: '#c45500', margin: '-10px 0 15px 0', fontFamily: 'monospace' }}>Target ID: {editForm.id}</p>

            <form onSubmit={handleEditFormSubmit} className="auth-form-flow">
              <div className="auth-input-group">
                <label htmlFor="edit_name">Product Name</label>
                <input
                  type="text" id="edit_name" name="name"
                  value={editForm.name} onChange={handleEditChange} required
                />
              </div>

              {/* Added Discount Row alongside Price and Stock inside the layout grid container */}
              <div className="auth-form-row-grid">
                <div className="auth-input-group">
                  <label htmlFor="edit_price">Price (INR)</label>
                  <input
                    type="number" id="edit_price" name="price" min="0"
                    value={editForm.price} onChange={handleEditChange} required
                  />
                </div>
                <div className="auth-input-group">
                  <label htmlFor="edit_discount">Discount (%)</label>
                  <input
                    type="number" id="edit_discount" name="discount" min="0" max="100"
                    value={editForm.discount} onChange={handleEditChange} required
                  />
                </div>
                <div className="auth-input-group">
                  <label htmlFor="edit_stock">Stock Units</label>
                  <input
                    type="number" id="edit_stock" name="stock" min="0"
                    value={editForm.stock} onChange={handleEditChange} required
                  />
                </div>
              </div>

              <div className="auth-input-group">
                <label htmlFor="edit_thumbnail">Replace Image File (Optional)</label>
                <input
                  type="file" id="edit_thumbnail" accept="image/*"
                  onChange={(e) => e.target.files?.[0] && setEditThumbnail(e.target.files[0])}
                />
              </div>

              <div className="auth-input-group">
                <label htmlFor="edit_description">Product Specification Description</label>
                <textarea
                  id="edit_description" name="description" rows="3" className="admin-textarea-field"
                  value={editForm.description} onChange={handleEditChange} required
                />
              </div>

              <div className="auth-form-row-grid" style={{ marginTop: '8px' }}>
                <button type="button" className="auth-secondary-create-btn" onClick={() => setEditingProduct(null)}>
                  Abort
                </button>
                <button type="submit" className="auth-action-btn-gold" style={{ margin: 0 }}>
                  Commit Changes
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

    </div>
  );
};

export default ProductAdd;