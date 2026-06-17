import React, { useContext, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import logo from '../assets/Amazon-Logo.png';
import UserContext from '../context/UserContext';
import api from '../api/axiosConfig'; // 📡 Centralized Axios instance with interceptors

const Login = () => {
  // Master switch toggling between standalone card components
  const [isLoginView, setIsLoginView] = useState(true);

  return (
    <div className="auth-page-container">
      {/* Centered Amazon Logo Header */}
      <div className="auth-logo-header">
        <Link to="/">
          <img src={logo} alt="Amazon Logo Black" className="auth-brand-logo" />
        </Link>
      </div>

      {/* Conditionally mount the independent form blocks */}
      {isLoginView ? (
        <LoginCard switchToRegister={() => setIsLoginView(false)} />
      ) : (
        <RegisterCard switchToLogin={() => setIsLoginView(true)} />
      )}

      {/* Shared Authentication Footer Links */}
      <footer className="auth-minimal-footer">
        <div className="auth-footer-links">
          <a href="#conditions">Conditions of Use</a>
          <a href="#privacy">Privacy Notice</a>
          <a href="#help">Help</a>
        </div>
        <p className="auth-footer-copyright">
          &copy; 1996-{new Date().getFullYear()}, Amazon.com, Inc. or its affiliates
        </p>
      </footer>
    </div>
  );
};

/* ==========================================================================
   📦 STANDALONE COMPONENT 1: LOGIN CARD
   ========================================================================== */
const LoginCard = ({ switchToRegister }) => {
  const { setUser } = useContext(UserContext);
  const navigate = useNavigate();

  const [loginData, setLoginData] = useState({
    email: '',
    password: '',
    stayLoggedIn: false
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setLoginData({
      ...loginData,
      [name]: type === 'checkbox' ? checked : value
    });
  };

  const handleLoginSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      // API request utilizing raw JSON body mapping
      const response = await api.post('/v1/Account/Login', loginData);
      
      const { jwtToken, refreshToken, email, firstName, lastName } = response.data;

      // Secure token tracking initialization
      localStorage.setItem('token', jwtToken);
      localStorage.setItem('refreshToken', refreshToken);

      // Hydrate global state context 
      setUser({ email:email, name: `${firstName} ${lastName}` });

      console.log('Login success! Session initialized.');
      navigate('/'); 
    } catch (err) {
      console.error('Login engine failed:', err);
      setError(err.response?.data?.error || 'Invalid email or password.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-card-box">
      <h1 className="auth-card-title">Sign in</h1>

      {error && <div className="auth-error-alert-box">{error}</div>}

      <form onSubmit={handleLoginSubmit} className="auth-form-flow">
        <div className="auth-input-group">
          <label htmlFor="login-email">Email</label>
          <input
            type="email"
            id="login-email"
            name="email"
            value={loginData.email}
            onChange={handleChange}
            disabled={loading}
            required
          />
        </div>

        <div className="auth-input-group">
          <label htmlFor="login-password">Password</label>
          <input
            type="password"
            id="login-password"
            name="password"
            value={loginData.password}
            onChange={handleChange}
            disabled={loading}
            required
          />
        </div>

        <div className="auth-checkbox-group">
          <input
            type="checkbox"
            id="login-stay"
            name="stayLoggedIn"
            checked={loginData.stayLoggedIn}
            onChange={handleChange}
            disabled={loading}
          />
          <label htmlFor="login-stay">Keep me signed in</label>
        </div>

        <button type="submit" className="auth-action-btn-gold" disabled={loading}>
          {loading ? 'Signing in...' : 'Continue'}
        </button>
      </form>

      <p className="auth-legal-disclaimer">
        By continuing, you agree to AmazonWeb's Clone <a href="#conditions">Conditions of Use</a> and <a href="#privacy">Privacy Notice</a>.
      </p>

      <div className="auth-toggle-context-tray">
        <div className="auth-divider-break">
          <h5>New to Amazon?</h5>
        </div>
        <button type="button" className="auth-secondary-create-btn" onClick={switchToRegister} disabled={loading}>
          Create your Amazon account
        </button>
      </div>
    </div>
  );
};

/* ==========================================================================
   📦 STANDALONE COMPONENT 2: REGISTER CARD (FIXED GRID LAYOUT)
   ========================================================================== */
const RegisterCard = ({ switchToLogin }) => {
  const { setUser } = useContext(UserContext);
  const navigate = useNavigate();

  const [registerData, setRegisterData] = useState({
    email: '',
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    password: '',
    confirmPassword: '',
    gender: '0',
    stayLoggedIn: false
  });

  const [loading, setLoading] = useState(false);
  const [errorFields, setErrorFields] = useState({});
  const [generalError, setGeneralError] = useState(null);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setRegisterData({
      ...registerData,
      [name]: type === 'checkbox' ? checked : value
    });
  };

  const handleRegisterSubmit = async (e) => {
    e.preventDefault();
    
    if (registerData.password !== registerData.confirmPassword) {
      setErrorFields({ confirmPassword: ['Passwords do not match.'] });
      return;
    }

    setLoading(true);
    setErrorFields({});
    setGeneralError(null);

    // ✨ Clean JSON Payload layout matching your successful Postman structure
    const registerPayload = {
      ...registerData,
      gender: parseInt(registerData.gender, 10),
      dateOfBirth: registerData.dateOfBirth ? new Date(registerData.dateOfBirth).toISOString() : null
    };

    try {
      // Firing pure application/json structure directly
      const response = await api.post('/v1/Account/Register', registerPayload);
      
      const { jwtToken, refreshToken, email, firstName, lastName } = response.data;

      localStorage.setItem('token', jwtToken);
      localStorage.setItem('refreshToken', refreshToken);

      setUser({ email, name: `${firstName} ${lastName}` });

      console.log('Account registered via JSON and verified successfully.');
      navigate('/');
    } catch (err) {
      console.error('Registration subsystem failed:', err);
      
      if (err.response?.data?.errors) {
        // Maps backend ModelState error strings seamlessly underneath input inputs
        setErrorFields(err.response.data.errors);
      } else {
        setGeneralError(err.response?.data?.error || 'An error occurred during account creation.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-card-box register-card-wide">
      <h1 className="auth-card-title">Create account</h1>

      {generalError && <div className="auth-error-alert-box">{generalError}</div>}

      <form onSubmit={handleRegisterSubmit} className="auth-form-flow">

        {/* ROW 1: First Name & Last Name sitting cleanly side-by-side */}
        <div className="auth-form-row-grid">
          <div className="auth-input-group">
            <label htmlFor="firstName">First name</label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              maxLength={50}
              value={registerData.firstName}
              onChange={handleChange}
              disabled={loading}
              required
            />
            {errorFields.FirstName && <span className="input-validation-err">{errorFields.FirstName[0]}</span>}
          </div>
          <div className="auth-input-group">
            <label htmlFor="lastName">Last name</label>
            <input
              type="text"
              id="lastName"
              name="lastName"
              maxLength={50}
              value={registerData.lastName}
              onChange={handleChange}
              disabled={loading}
              required
            />
            {errorFields.LastName && <span className="input-validation-err">{errorFields.LastName[0]}</span>}
          </div>
        </div>

        {/* ROW 2: Date of Birth & Gender Selection alignment */}
        <div className="auth-form-row-grid">
          <div className="auth-input-group">
            <label htmlFor="dateOfBirth">Date of birth</label>
            <input
              type="date"
              id="dateOfBirth"
              name="dateOfBirth"
              value={registerData.dateOfBirth}
              onChange={handleChange}
              disabled={loading}
            />
            {errorFields.DateOfBirth && <span className="input-validation-err">{errorFields.DateOfBirth[0]}</span>}
          </div>
          <div className="auth-input-group">
            <label htmlFor="gender">Gender</label>
            <select
              id="gender"
              name="gender"
              value={registerData.gender}
              onChange={handleChange}
              className="auth-select-field"
              disabled={loading}
              required
            >
              <option value="0">Male</option>
              <option value="1">Female</option>
              <option value="2">Other</option>
            </select>
          </div>
        </div>

        {/* Full Width Standard Inputs */}
        <div className="auth-input-group">
          <label htmlFor="reg-email">Email</label>
          <input
            type="email"
            id="reg-email"
            name="email"
            value={registerData.email}
            onChange={handleChange}
            disabled={loading}
            required
          />
          {errorFields.Email && <span className="input-validation-err">{errorFields.Email[0]}</span>}
        </div>

        <div className="auth-input-group">
          <label htmlFor="reg-password">Password</label>
          <input
            type="password"
            id="reg-password"
            name="password"
            minLength={6}
            maxLength={100}
            placeholder="At least 6 characters"
            value={registerData.password}
            onChange={handleChange}
            disabled={loading}
            required
          />
          {errorFields.Password && <span className="input-validation-err">{errorFields.Password[0]}</span>}
        </div>

        <div className="auth-input-group">
          <label htmlFor="confirmPassword">Password again</label>
          <input
            type="password"
            id="confirmPassword"
            name="confirmPassword"
            value={registerData.confirmPassword}
            onChange={handleChange}
            disabled={loading}
            required
          />
          {errorFields.confirmPassword && <span className="input-validation-err">{errorFields.confirmPassword[0]}</span>}
        </div>

        {/* Catches administrative block errors or role violations from the API */}
        {errorFields.UserRole && (
          <div className="auth-error-alert-box inner-form-alert">
            {errorFields.UserRole[0]}
          </div>
        )}

        <div className="auth-checkbox-group">
          <input
            type="checkbox"
            id="reg-stay"
            name="stayLoggedIn"
            checked={registerData.stayLoggedIn}
            onChange={handleChange}
            disabled={loading}
          />
          <label htmlFor="reg-stay">Keep me signed in</label>
        </div>

        <button type="submit" className="auth-action-btn-gold" disabled={loading}>
          {loading ? 'Creating Account...' : 'Add Account'}
        </button>
      </form>

      <p className="auth-legal-disclaimer">
        By creating an account, you agree to AmazonWeb's Clone <a href="#conditions">Conditions of Use</a> and <a href="#privacy">Privacy Notice</a>.
      </p>

      <div className="auth-toggle-context-tray style-inline">
        <p>
          Already have an account?{' '}
          <span className="auth-switch-link" onClick={switchToLogin}>
            Sign in ➔
          </span>
        </p>
      </div>
    </div>
  );
};

export default Login;