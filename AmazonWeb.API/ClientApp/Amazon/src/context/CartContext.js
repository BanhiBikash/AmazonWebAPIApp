import React,{createContext,useContext} from "react";

const CartContext = createContext()

export const useCart = () => {
    const context = useContext(CartContext);
    if (!context) {
        throw new Error("useCart must be consumed inside a valid CartContextProvider container shell.");
    }
    return context;
};

export default CartContext;