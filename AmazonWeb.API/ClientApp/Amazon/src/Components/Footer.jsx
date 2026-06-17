import React from 'react';
import { Link } from 'react-router-dom';
import logo from '../assets/Amazon-Logo.png'; // Reuses your navbar logo asset

const Footer = () => {
  // Smooth scroll handler to return smoothly to the top of the browser page
  const scrollToTop = () => {
    window.scrollTo({
      top: 0,
      behavior: 'smooth',
    });
  };

  return (
    <footer className="footer-container">
      {/* Back to top Action Banner */}
      <div className="back-to-top" onClick={scrollToTop}>
        Back to top
      </div>

      {/* Main Link Columns Section */}
      <div className="footer-links-grid">
        <div className="footer-column">
          <h3>Get to Know Us</h3>
          <ul>
            <li><a href="#about">About Us</a></li>
            <li><a href="#careers">Careers</a></li>
            <li><a href="#press">Press Releases</a></li>
          </ul>
        </div>

        <div className="footer-column">
          <h3>Connect with Us</h3>
          <ul>
            <li><a href="#fb">Facebook</a></li>
            <li><a href="#tw">X / Twitter</a></li>
            <li><a href="#ig">Instagram</a></li>
          </ul>
        </div>

        <div className="footer-column">
          <h3>Make Money with Us</h3>
          <ul>
            <li><Link to="/account/login">Sell on AmazonWeb</Link></li>
            <li><a href="#affiliate">Become an Affiliate</a></li>
            <li><a href="#advertise">Advertise Your Products</a></li>
          </ul>
        </div>

        <div className="footer-column">
          <h3>Let Us Help You</h3>
          <ul>
            <li><Link to="/account/login">Your Account</Link></li>
            <li><a href="#returns">Returns Centre</a></li>
            <li><a href="#help">Help & Support</a></li>
          </ul>
        </div>
      </div>

      <hr className="footer-divider" />

      {/* Brand Branding Identity Row */}
      <div className="footer-brand-row">
        <img src={logo} alt="AmazonWeb Logo" className="footer-logo" onClick={scrollToTop} />
      </div>

      {/* Fine-print Attribution Baseline */}
      <div className="footer-baseline">
        <p>&copy; {new Date().getFullYear()} AmazonWeb Clone Application. Built with React & ASP.NET Core.</p>
      </div>
    </footer>
  );
};

export default Footer;