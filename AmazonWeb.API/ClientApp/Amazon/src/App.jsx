import './App.css'
import { BrowserRouter, createBrowserRouter, createRoutesFromElements, Route, Router, RouterProvider } from 'react-router-dom';
import { Amazon } from './Amazon'
import Home from "./pages/Home"
import Login from "./pages/Login"
import Product from "./pages/Product"
import ProductAdd from './pages/AddProduct';
import Account from './pages/Account';
import Account_Update from './pages/Account_Update';
import Cart from "./pages/Cart"
import Checkout from './pages/Checkout';
import CheckoutDemo from './pages/CheckoutDemo';
import SearchResult from './pages/SearchResult';
import Order_Success from './pages/Order_Success';
import Orders from './pages/Orders';
import Order from './pages/Order';
import Categories from "./pages/Categories"

function App() {
  // 1. Declare your layout routes cleanly inside your browser router creation hook
  const router = createBrowserRouter(
    createRoutesFromElements(
      <Route path="/" element={<Amazon />}>
        <Route index element={<Home />} />                {/* Matches layout root "/" */}
        <Route path="Categories" element={<Categories />} />    {/* Categories Url */}
        <Route path="account" element={<Account />} />                {/* Matches layout root "/" */}
        <Route path="Account_Update" element={<Account_Update />} />                {/* Matches layout root "/" */}
        <Route path="login" element={<Login />} />        {/* Matches "/login" */}
        <Route path="product/:id" element={<Product />} />    {/* Matches "/product" */}
        <Route path="add_product" element={<ProductAdd />} />    {/* Matches "/add_product" */}
        <Route path="Cart" element={<Cart />} />    {/* Matches "/add_product" */}
        <Route path="Checkout/:id?" element={<Checkout />} /> {/* The direct, express gateway page bypass */}
        <Route path="CheckoutDemo/:id?" element={<CheckoutDemo />} /> {/* The direct, express gateway page bypass */}
        <Route path="SearchResult" element={<SearchResult />} />
        <Route path="Order_Success" element={<Order_Success />}/>
        <Route path="Orders" element={<Orders />} />
        <Route path="Order" element={<Order />} />
      </Route>
    )
  );

  // 2. Return the standalone provider passing your config down using the router property 
  return <RouterProvider router={router} />;
}

export default App
