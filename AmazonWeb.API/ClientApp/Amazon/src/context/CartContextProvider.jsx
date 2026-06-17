import React, { useContext, useEffect, useState } from "react"
import CartContext from "./CartContext"
import UserContext from "./UserContext"
import api from "../api/axiosConfig"

const CartContextProvider = ({ children }) => {

    const [cart, setCart] = useState({
        cart: [],
        isBusy: false
    })

    const { user } = useContext(UserContext)

    // 🎯 FIXED: Added 'async' so that 'await api.get' compiles cleanly
    async function fetchInitialCartData() {

        // Cart is busy fetching initial data
        setCart(prev => { return { ...prev, isBusy: true } })

        // Either user is not set or email Id is not there, use local storage
        if (!user || !user.email) {
            const localCart = localStorage.getItem('guest_cart')

            if (localCart) {
                // 🎯 FIXED: Moved try/catch to sit completely around JSON.parse()
                try {
                    const parsedLocalCart = JSON.parse(localCart)
                    setCart({
                        cart: Array.isArray(parsedLocalCart) ? parsedLocalCart : [],
                        isBusy: false
                    })
                }
                catch (e) {
                    console.log("failed to parse local cart!", e)
                    setCart({ cart: [], isBusy: false })
                }
            } else {
                setCart({ cart: [], isBusy: false })
            }
        }
        // User is found, look for DB cart and handle sync workflows
        else {
            try {
                // 1. Retrieve any leftover items from guest browsing session
                const localCart = localStorage.getItem('guest_cart');
                const parsedLocalCart = localCart ? JSON.parse(localCart) : [];

                let response;

                // 2. 🎯 CRITICAL: If guest items exist, hit the backend merge endpoint first
                if (Array.isArray(parsedLocalCart) && parsedLocalCart.length > 0) {

                    // Map the frontend structure to the exact C# CartRequest model expected by [FromBody] List<CartRequest>
                    const mergePayload = parsedLocalCart.map(item => ({
                        productId: item.productId,
                        quantity: item.quantity || 1
                    }));

                    console.log("Merging guest cart records into your database account tracking profile...");

                    // Post payload to backend. The token interceptor handles auth headers automatically!
                    response = await api.post('/v1/Cart/MergeCart', mergePayload);
                    
                    // 🧹 Clear the guest storage out immediately so we don't merge them again on future re-renders
                    localStorage.removeItem('guest_cart');

                } else {
                    // No guest items found, execute standard catalog retrieval route safely
                    response = await api.get('/v1/Cart');
                }
                //check what the response is
                console.log(response)
                // 3. Extract the clean backend item list array from your DTO payload structure
                const backendCartArray = response.data?.items || [];

                setCart({
                    cart: Array.isArray(backendCartArray) ? backendCartArray : [],
                    isBusy: false
                });

            } catch (err) {
                console.error("Failed to sync or merge authenticated cart array from backend service layer:", err);
                setCart({ cart: [], isBusy: false });
            }
        }
    }

    useEffect(function () {
        fetchInitialCartData()
    }, [user])

    return (
        <CartContext.Provider value={{ cart, setCart }}>
            {children}
        </CartContext.Provider>
    )
}

export default CartContextProvider