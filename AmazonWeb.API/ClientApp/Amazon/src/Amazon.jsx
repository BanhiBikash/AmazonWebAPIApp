import React from "react";
import Navbar from "./Components/Navbar";
import Footer from "./Components/Footer"
import { Outlet } from "react-router-dom";
import UserContextProvider from "./context/UserContextProvider";
import CartContextProvider from "./context/CartContextProvider";
import CategoryContextProvider from "./context/CategoryContextProvider";

export const Amazon = () => {
    return (
        <UserContextProvider><CartContextProvider><CategoryContextProvider>
            <div className="app-layout-wrapper">
                {/* Locked to viewport top */}
                <Navbar />

                {/* Dynamic page context insertion area */}
                <main className="main-content-fluid">
                    <Outlet />
                </main>

                {/* Base alignment ground layer */}
                <Footer />
            </div>
        </CategoryContextProvider></CartContextProvider></UserContextProvider>
    )
}