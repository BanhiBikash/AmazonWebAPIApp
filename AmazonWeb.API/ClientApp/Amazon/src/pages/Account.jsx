import React, { useContext } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import UserContext from '../context/UserContext';

const Account = () => {
  const { user, setUser } = useContext(UserContext);
  const navigate = useNavigate();

  // Route protection fallback if user drops in manually via address bar typing
  if (!user) {
    return (
      <div className="account-dashboard-fallback">
        <h2>Please log in to manage your account details.</h2>
        <Link to="/login" className="auth-action-btn-gold">Sign In</Link>
      </div>
    );
  }

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    setUser(null);
    console.log('Session destroyed from Account Dashboard panel context.');
    navigate('/');
  };

  return (
    <div className="account-dashboard-container">
      <div className="account-dashboard-header">
        <h1>Your Account Dashboard</h1>
        <p>Manage settings, monitor inventory orders, and configure support parameters for **{user.name}** ({user.email})</p>
      </div>

      {/* Grid Layout containing functional navigation modules */}
      <div className="account-dashboard-grid">
        
        {/* Module 1: Profile Modifications */}
        <Link to="../Account_Update" className="account-dash-card">
          <div className="dash-card-icon-box">👤</div>
          <h3>Login & Security</h3>
          <p>Edit name, update baseline delivery communication strings, or change authentication passwords.</p>
        </Link>

        {/* Module 2: Order Receipts */}
        <Link to="/orders" className="account-dash-card">
          <div className="dash-card-icon-box">📦</div>
          <h3>Your Orders</h3>
          <p>Track packages, examine histories, or process invoice details for recent purchases.</p>
        </Link>

        {/* Module 3: Help Desk Integration */}
        <Link to="/customer-service" className="account-dash-card">
          <div className="dash-card-icon-box">🎧</div>
          <h3>Customer Service</h3>
          <p>Open dialogue tickets, look into account status restrictions, or talk with system administrators.</p>
        </Link>

      </div>

      {/* Distinct System Action Row for Clean Session Management */}
      <div className="account-dashboard-action-tray">
        <button onClick={handleLogout} className="account-dash-logout-btn">
          Sign Out of Session
        </button>
      </div>
    </div>
  );
};

export default Account;