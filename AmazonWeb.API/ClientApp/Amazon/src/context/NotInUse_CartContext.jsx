import React, { createContext, useContext, useEffect, useState } from 'react';
import api from '../api/axiosConfig';
import UserContext from './UserContext'; // Clean default import

// 1. 🎯 Define the safe fallback blueprint object directly
const defaultCartState = {
  cart: [
    {
      id: 1,
      productId: 1,
      name: "Test Wireless Earbuds",
      price: 2999,
      quantity: 2 // This should contribute 2 to the count
    },
    {
      id: 2,
      productId: 2,
      name: "Test Mechanical Keyboard",
      price: 4500,
      quantity: 1 // This should contribute 1 to the count
    }
  ],
  setCart: () => {},
  cartLoading: true,
  totalItemsCount: 0
};

// 2. 🔑 Create and export the raw context object
export const CartContext = createContext(defaultCartState);

// 3. 🛠️ Create our custom hook wrapper
export default function useCart() {
  return useContext(CartContext);
}

// ==========================================================================
// 🔄 FACTORY PROVIDER FUNCTION (Written normally to bypass Vite HMR checks)
// ==========================================================================
export function CartContextProvider({ children }) {
  const { user } = useContext(UserContext);
  const [cart, setCart] = useState([]);
  const [cartLoading, setCartLoading] = useState(true);

  // Synchronize cart data whenever authentication status changes
  useEffect(() => {
    const fetchActiveCart = async () => {
      setCartLoading(true);
      
      if (user && user.id) {
        try {
          const response = await api.get(`/v1/Cart?userId=${user.id}`);
          setCart(response.data || []);
        } catch (err) {
          console.error("Failed to load database cart:", err);
        }
      } else {
        const localCartRaw = localStorage.getItem('guest_cart');
        setCart(localCartRaw ? JSON.parse(localCartRaw) : []);
      }
      console.log({cart})
      setCartLoading(false);
    };

    fetchActiveCart();
  }, [user]);

  // Listen to localStorage updates across tabs/components for guest mode
  useEffect(() => {
    const handleStorageChange = () => {
      if (!user || !user.id) {
        const localCartRaw = localStorage.getItem('guest_cart');
        setCart(localCartRaw ? JSON.parse(localCartRaw) : []);
      }
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, [user]);

  return (
    <CartContext.Provider value={{ cart, setCart, cartLoading }}>
      {children}
    </CartContext.Provider>
  );
}