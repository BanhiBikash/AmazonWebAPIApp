import React, { useContext, useEffect, useState } from 'react'
import CategoriesRow from '../Components/CategoriesRow'
import CategorySubCategory from '../context/CategorySubCategory'
import api from '../api/axiosConfig'
import ProductRow from "../Components/ProductRow"

const Categories = () => {
  const context = useContext(CategorySubCategory) || {};
  const { category = { categoryArray: [], subCategoryArray: [] } } = context;
  const { categoryArray = [] } = category;

  // ✅ FIX 1: Use state so that the pulled row data survives component re-renders
  const [productRows, setProductRows] = useState([]);

  useEffect(() => {
    async function getProductRows() {
      // Safety guard: If context data hasn't loaded yet, wait for the next lifecycle tick
      if (!categoryArray || categoryArray.length === 0) return;

      // Temporary array to collect API results safely before modifying component state
      const tempRows = [];

      for (let i = 0; i < categoryArray.length; i++) {
        // ✅ FIX 2: Ensure the object index exists before trying to destructure 'name'
        if (categoryArray[i]) {
          const { name } = categoryArray[i];
          try {
            const response = await api.get(`v1/Products/category/${name}`);
            // Axios returns the data body inside response.data. Adjust if your interceptor alters this.
            tempRows.push(response.data || response); 
          } catch (error) {
            console.error(`Failed to fetch product rows for category: ${name}`, error);
          }
        }
      }

      // ✅ FIX 3: Push the collected payload items into the state engine to safely trigger UI updates
      setProductRows(tempRows);
    }

    getProductRows();
  }, [categoryArray]); // Triggers cleanly whenever categoryArray shifts from empty to filled

  return (
    <>
      {/* top categories row  */}
      <CategoriesRow />

      {/* Product Rows */}
      {/* ✅ FIX 4: Changed arrow function block from curly braces {} to parentheses () 
          so it actually returns the JSX template to the DOM. Also added a unique 'key' prop. */}
      {productRows.map((row, index) => (
        (row.length>0 && <ProductRow key={row.id || index} row={row} categoryName={row.name} />)
      ))}
    </>
  )
}

export default Categories;