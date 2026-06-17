import React, { useContext, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import logo from '../assets/Amazon-Logo.png';
import nav_icon from "../assets/hamburger.png";
import UserContext from '../context/UserContext';
import userLogo from "../assets/user.png";
import cartLogo from "../assets/cart.png";
import { useCart } from '../context/CartContext';
import api from '../api/axiosConfig';
import CategorySubCategory from '../context/CategorySubCategory';

const Navbar = () => {
  const { user } = useContext(UserContext);
  const { cart } = useCart();
  const {category, setCategory} = useContext(CategorySubCategory);
  const {categoryArray,subCategoryArray} = category;
  const navigate = useNavigate();

  // 🔍 Search bar input and category state filters
  const [searchQuery, setSearchQuery] = useState('');
  const [searchCategory, setSearchCategory] = useState('All');

  // 📦 Fetch dynamic category metadata on navbar mount
  useEffect(() => {
    const fetchMetadata = async () => {
       
      try {
        const response = await api.get('v1/Products/GetCategories');
        const { categories, subCategories } = response.data;

        // Set categories state array safely
        setCategory({categoryArray:categories, subCategoryArray:subCategories});
         
      } catch (e) {
        console.log("Error: can't fetch category " + e);
      }
    };

    fetchMetadata();
  }, []);

  const getFirstName = () => {
    if (!user || !user.name) return 'Account';
    return user.name.split(' ')[0];
  };

  const getTotalCartCount = () => {
    if (!cart || !cart.cart || !Array.isArray(cart.cart)) return 0;
    return cart.cart.reduce((total, item) => total + (item.quantity || 0), 0);
  };

  const totalCount = getTotalCartCount();

  // ⚡ Handle Form Submit Navigation 
  const handleSearchSubmit = (e) => {
    e.preventDefault();

    const trimmedQuery = searchQuery.trim();

    //if nothing is typed return
    if (!trimmedQuery) return;

    const params = new URLSearchParams();

    //append the input
    params.append('q', trimmedQuery);

    //empty the input text
    setSearchQuery('')

    // Directs browser routing target using the exact matching casing matching your App routes ("SearchResult")
    navigate(`/SearchResult?${params.toString()}`);
  };

  //search by Category
  const searchByCategory = (e)=>{
    //set the search category
    setSearchCategory(e.target.value)
    
    const params = new URLSearchParams();
    params.append('category',e.target.value)

    //empty the input text
    setSearchQuery('')

    // Directs browser routing target using the exact matching casing matching your App routes ("SearchResult")
    navigate(`/SearchResult?${params.toString()}`);
  }

  return (
    <div className="Navbar">

      {/* Brand Icon Layout Sections */}
      <div className='icons'>
        <img
          src={nav_icon}
          alt="Toggle Menu"
          className="Nav-linksLogo-mobile"
          onClick={() => console.log('Mobile menu toggled')}
        />

        <Link to="/" className="logo-link">
          <img className="logo" src={logo} alt="AmazonWeb Logo" />
        </Link>
      </div>

      {/* 🔍 Amazon-Style Core Search Bar Container Engine */}
      <form className="nav-search-bar-container" onSubmit={handleSearchSubmit}>
        <select
          className="nav-search-dropdown"
          value={searchCategory}
          onChange={(e) => searchByCategory(e)}
        >
          <option value="All">All Categories</option>
          {Array.isArray(categoryArray) && categoryArray.map((item, index) => (
            <option value={item.name || item} key={item.id || item.name || index}>
              {item.name || item}
            </option>
          ))}
        </select>

        <input
          type="text"
          className="nav-search-input"
          placeholder="Search AmazonWeb..."
          value={searchQuery}
          onChange={(e) => { setSearchQuery(e.target.value) }}
        />

        <button type="submit" className="nav-search-submit-btn">
          {/* Magnifying glass icon layout */}
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="20" height="20" fill="currentColor">
            <path d="M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z" />
          </svg>
        </button>
      </form>

      {/* Navigation Links Routing Container */}
      <div className="nav-links">
        <Link to="/" className="nav-items text-link-node">Home</Link>
        <Link to="/categories" className="nav-items text-link-node">Categories</Link>

        {user && (
          <Link to="/add_product" className="nav-items text-link-node">Handle Products</Link>
        )}

        {user ? (
          <Link to="/Account" className="nav-items-nav-account-link-profile">
            <span className="nav-profile-firstname">{getFirstName()}</span>
            <img className="nav-profile-arrow-icon" src={userLogo} alt="User" />
          </Link>
        ) : (
          <Link to="/login" className="nav-items text-link-node">Login</Link>
        )}

        <Link to="/Cart" className='cartLogo'>
          <div className="cart-icon-wrapper">
            <img src={cartLogo} alt="Cart Logo" />
            {totalCount > 0 && (
              <span className="nav-cart-badge-count">{totalCount}</span>
            )}
          </div>
        </Link>
      </div>

    </div>
  );
};

export default Navbar;