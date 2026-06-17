import React, { useContext, useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import CategorySubCategory from '../context/CategorySubCategory';
import api from '../api/axiosConfig';

const CategoriesRow = () => {
  const navigate = useNavigate();
  
  // Destructure safely with a fallback default context object
  const context = useContext(CategorySubCategory) || {};
  const { category = { categoryArray: [], subCategoryArray: [] } } = context;
  const { categoryArray } = category;

  // ✅ FIXED: Using local state to manage asynchronous API values safely
  const [productArray, setProductArray] = useState([]);
  const [loading, setLoading] = useState(true);

  // ✅ FIXED: Fetching inside useEffect to prevent infinite rendering network loops
  useEffect(() => {
    const fetchProductArray = async () => {
      try {
        const response = await api.get('v1/products/GetFirstProductEachCategory');
        // Ensure we store the raw data payload array safely
        setProductArray(response.data || []);
      } catch (error) {
        console.error("Error fetching category thumbnail icons:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchProductArray();
  }, []);

  if (loading) {
    return <div className="categories-placeholder">Loading categories...</div>;
  }

  return (
    <div className='categories'>
      {/* Heading */}
      <h2>Top Categories</h2>

      {/* Categories Row */}
      <div className="categories-row">
        {
          categoryArray.map(cat => {
            // ✅ FIXED: Find the matching product image dynamically by enum/name property
            // This is safer than an incremental loop index because order mismatches won't break it
            const matchingProduct = productArray.find(
              prod => prod.category === cat.name || String(prod.category) === String(cat.id)
            );

            // Fallback placeholder image if a category doesn't have any products yet
            const displayImageUrl = matchingProduct?.imageUrl;
            
            return (
              <div 
                className="categoryAndName" 
                key={cat.id || cat.categoryId}
                onClick={() => navigate(`/SearchResult?category=${cat.name}`)}
              >
                {/* Circular Category Icon Bounding Box */}
                <div className="category">
                  <img src={displayImageUrl} alt={cat.name} />
                </div>
                
                <span>{cat.name}</span>
              </div>
            );
          })
        }
      </div>
    </div>
  );
};

export default CategoriesRow;