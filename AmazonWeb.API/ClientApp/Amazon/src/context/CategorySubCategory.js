import { createContext } from 'react';

// 1. Create the context container
const CategorySubCategory = createContext();

// 2. Export it cleanly so providers and components can consume it
export default CategorySubCategory;