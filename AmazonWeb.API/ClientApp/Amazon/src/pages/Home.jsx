import React, { useContext, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/axiosConfig";
import UserContext from "../context/UserContext";
import Quad from "../Components/Quad";
import ProductRow from "../Components/ProductRow";
import Banner from "../Components/Banner";

const Home = () => {

  //get user
  const { user } = useContext(UserContext)

  const [quad, setQuad] = useState([])
  const [quad2, setQuad2] = useState([])
  const [row, setRow] = useState([])

  async function getQuad() {
    //get quad items
    const quad_items = await api.get('/v1/Products/category/Furniture');

    if (quad_items != null) {
      console.log("data received")
    }
     
    setQuad(quad_items.data)
  }

  async function getQuad2() {
    //get quad items
    const quad_items = await api.get('/v1/Products/category/HomeAppliances');

    if (quad_items != null) {
      console.log("data received")
    }
     
    setQuad2(quad_items.data)
  }

  async function getRow() {

    //get itens
    const response = await api.get('/v1/Products/category/Mobiles');

    if (response != null) {
      console.log("data received mobile")
    }
     
    setRow(response.data)
  }

  //load the items on render
  useEffect(function () {
    getQuad(); getQuad2(); getRow();
  }, [])

  return (
    <div className="home-page-container">
      {/* 1. HERO BANNER BACKGROUND CAROUSEL */}
      <Banner />

      {/* 2. OVERLAPPING OVERVIEW HUB MATRIX GRID */}
      <div className="home-content-grid">

        {/* Card 1: Quad Component layout */}
        <Quad items = {quad} referTo="/login" topic = "Revamp your home | Up to 60% off" />

        {/* Card 2: Single Large Item display */}
        <div className="product-card-container">
          <h2 className="card-title">Latest Devices | Fire TV & Echo</h2>
          <div className="single-image-wrapper">
            <img
              src="https://helios-i.mashable.com/imagery/comparisons/00fRDHtInqzkLrGIuQ4dwtw-item2.fit_lim.size_1028x578.v1742572179.png"
              alt="Amazon Echo Devices"
              className="single-card-img"
            />
          </div>
          <Link to="/product" className="card-explore-link">Explore smart features</Link>
        </div>

        {/* Card 2: Quad Component layout */}
        <Quad items = {quad2} referTo="/orders" topic = "Your home Electronics | Up to 60% off" />

        {/* Card 4: Quick Sign-In Module Callout - if no user */}
        {!user && (
          <div className="product-card-container gateway-auth-promo">
            <div className="promo-inner-block">
              <h2 className="card-title">Sign in for your best experience</h2>
              <Link to="/login" className="amazon-primary-btn">Sign in securely</Link>
            </div>
            <div className="promo-banner-footer-img">
              <img
                src="data:image/webp;base64,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"
                alt="Amazon Pay Integration Promo"
              />
            </div>
          </div>
        )}


      </div>

      {/* 3. BREAKOUT SINGLE ROW: WIDE DEALS SLIDER TIER */}
      <ProductRow row = {row} />
    </div>
  );
};

export default Home;