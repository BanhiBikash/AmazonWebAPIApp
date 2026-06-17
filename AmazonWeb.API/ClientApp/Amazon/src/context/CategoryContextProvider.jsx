import React, { useState} from 'react'
import CategorySubCategory from './CategorySubCategory'

const CategoryContextProvider = ({ children }) => {
  // Keeping consistent with exact state blueprint
  const [category, setCategory] = useState({ categoryArray: [], subCategoryArray: [] })

  return (
    <CategorySubCategory.Provider value={{category, setCategory}}>
        {children}
    </CategorySubCategory.Provider>
  )
}

export default CategoryContextProvider