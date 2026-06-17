import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import logo from "../assets/Amazon-Logo.png";
import api from '../api/axiosConfig';
import { useParams } from 'react-router-dom';

const CheckoutDemo = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const [productArray, setProductArray] = useState([])
  const [checkoutAmount, setChekoutAmount] = useState(0)

  const { id } = useParams();
  const orderData = location.state?.orderData || { items: [], totalAmount: 0 };
  const { items, totalAmount } = orderData; 

  const [userProfile, setUserProfile] = useState({
    userId: '',
    name: 'Demo Customer',
    address: '',
    city: '',
    state: '',
    postalCode: '',
    country: '',
    isLoading: true
  });

  useEffect(() => {
    const fetchProfileDetails = async () => {
      const token = localStorage.getItem('token');
      if (!token) {
        setUserProfile(prev => ({ ...prev, isLoading: false }));
        return;
      }

      try {
        const response = await api.get('/v1/Account/getprofiledetails', {
          headers: { 'Authorization': `Bearer ${token}` }
        });

        const data = response.data;
        const fullName = data.firstName && data.lastName
          ? `${data.firstName} ${data.lastName}`
          : 'Demo Customer';

        setUserProfile({
          userId: data.userId || '',
          name: fullName,
          address: data.address || '',
          city: data.city || '',
          state: data.state || '',
          postalCode: data.postalCode || '',
          country: data.country || '',
          isLoading: false
        });
      } catch (err) {
        console.error("Failed to load user info for demo checkout:", err);
        setUserProfile(prev => ({ ...prev, isLoading: false }));
      }
    };

    const fetchProductData = async () => {
      try {
        // 🎯 FIX 1: Added leading absolute slash '/' to ensure correct routing pathing
        const response = await api.get(`/v1/Products/${id}`);
        const product = response.data;

        if (product) {
          // 🎯 FIX 2: Format the backend response properties right here to match 
          // your frontend component item schema constraints perfectly
          setProductArray([{
            productId: product.id,        // Maps database 'id' to 'productId'
            productName: product.name,    // Maps database 'name' to 'productName'
            imageUrl: product.imageUrl,
            quantity: 1,
            unitPrice: product.price      // Maps database 'price' to 'unitPrice'
          }]);
          setChekoutAmount(product.price ?? 0);
        }

      } catch (e) {
        console.log('failed to fetch product data with id:' + id);
      }
    }

    fetchProfileDetails();
    
    if (id) {
      fetchProductData();
    }

    if (!id) {
      setProductArray(items || []);
      setChekoutAmount(totalAmount || 0);
    }
  }, [id, items, totalAmount]);

  const handleDemoPaymentSubmit = async () => {
    const completedBackendPayload = {
      ShippingAddress: userProfile.address || "123 Demo St, Sandbox Sector",
      PostalCode: userProfile.postalCode || "700001",
      City: userProfile.city || "Demo City",
      Country: userProfile.country || "India",
      Items: productArray.map(item => ({
        ProductId: item.productId,
        ProductName: item.productName || "Demo Product",
        ImageUrl: item.imageUrl || "https://via.placeholder.com/80?text=Product",
        Quantity: parseInt(item.quantity, 10) || 1,
        UnitPrice: parseInt(item.unitPrice, 10) || 0
      }))
    };

    try {
      console.log("[DEMO FLOW] Submitting order payload:", completedBackendPayload);
      const response = await api.post('/v1/orders/ReceiveOrder', completedBackendPayload);

      if (response.status === 200 || response.status === 201) {
        const { id, totalAmount: backendAmount } = response.data;
        console.log("[DEMO FLOW] Order created on server. ID:", id);

        if (backendAmount > 0 && /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(id)) {

          const demoConfirmationPayload = {
            OrderId: id,
            RazorpayPaymentId: `pay_MOCK_${Math.random().toString(36).substring(2, 11).toUpperCase()}`,
            RazorpayOrderId: `order_MOCK_${Math.random().toString(36).substring(2, 11).toUpperCase()}`,
            RazorpaySignature: "SIMULATED_DEMO_HMAC_HASH_SIGNATURE_OK",
            PaymentMethod: "Wallet" 
          };

          console.log("[DEMO FLOW] Submitting simulated payment verification data:", demoConfirmationPayload);

          const confirmResponse = await api.post('/v1/Transaction/ConfirmPaymentDemo', demoConfirmationPayload);

          if (confirmResponse.status === 200 || confirmResponse.status === 201) {
            console.log("[DEMO FLOW] Payment mocked successfully. Moving to success screen.");
            navigate(`/order_success?orderNo=${confirmResponse.data.orderId}&totalAmount=${checkoutAmount}`);
          } else {
            alert("Demo confirmation endpoint rejected payload layout.");
          }
        } else {
          alert("Invalid order identifiers returned from database context.");
        }
      }
    } catch (error) {
      console.error("[DEMO FLOW] Execution crashed:", error);
      alert("Demo Checkout Halted: " + (error.response?.data || "Check console errors."));
    }
  };

  if (userProfile.isLoading) {
    return (
      <div style={styles.emptyContainer}>
        <h3>Loading your simulated delivery environment...</h3>
      </div>
    );
  }

  if (!productArray || productArray.length === 0) {
    return (
      <div style={styles.emptyContainer}>
        <h3>Your checkout context is empty.</h3>
        <button style={styles.primaryBtn} onClick={() => navigate('/')}>Return to Shopping</button>
      </div>
    );
  }

  return (
    <div style={styles.container}>
      <div style={{ textAlign: 'center', marginBottom: '10px' }}>
        <img src={logo} alt="Amazon Logo" style={{ height: '40px', objectFit: 'contain' }} />
        <div style={styles.demoBanner}>⚠️ DEMO BYPASS MODE ACTIVATED (NO REAL GATEWAY)</div>
      </div>
      <h2 style={styles.heading}>Review Your Order (Demo Check)</h2>

      <div style={styles.addressCard}>
        <div style={styles.addressHeader}>
          <h3 style={styles.sectionTitle}>Shipping Destination (Mock Profile)</h3>
        </div>
        <p style={styles.userName}>{userProfile.name}</p>
        <p style={styles.addressText}>{userProfile.address || '123 Demo St, Sandbox Sector'}</p>
        <p style={styles.addressText}>{userProfile.city} {userProfile.state} {userProfile.postalCode}</p>
      </div>

      <div style={styles.itemsCard}>
        <div style={styles.actionRow}>
          <button style={styles.paymentBtn} onClick={handleDemoPaymentSubmit}>
            Instant Demo Pay: ₹{checkoutAmount}
          </button>
        </div>

        <h3 style={styles.sectionTitle}>Review Items</h3>
        <div style={styles.itemsList}>
          {productArray.map((item) => (
            <div key={item.productId} style={styles.itemRow}>
              <img src={item.imageUrl} alt={item.productName} style={styles.productImg} onClick={function(){navigate(`../Product/${item.productId}`)}} />
              <div style={styles.itemDetails}>
                {/* 🎯 Now clean and safely consistent without local inline arrays queries */}
                <h4 style={styles.productName}>{item.productName}</h4>
                <p style={styles.productMeta}>Qty: <strong>{item.quantity}</strong></p>
                <p style={styles.productPrice}>₹{item.unitPrice.toLocaleString('en-IN')} each</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

const styles = {
  container: { maxWidth: '650px', margin: '20px auto', padding: '0 15px 40px 15px', fontFamily: 'sans-serif' },
  demoBanner: { background: '#d4edda', color: '#155724', padding: '6px', fontSize: '12px', fontWeight: 'bold', borderRadius: '4px', marginTop: '5px' },
  heading: { fontSize: '24px', fontWeight: '600', color: '#111', marginBottom: '20px' },
  addressCard: { background: '#fff', border: '1px solid #ddd', borderRadius: '8px', padding: '16px', marginBottom: '16px' },
  addressHeader: { display: 'flex', justifyContent: 'space-between', marginBottom: '12px' },
  sectionTitle: { fontSize: '16px', fontWeight: '600', color: '#222', margin: 0 },
  userName: { fontWeight: '600', fontSize: '15px', margin: '0 0 4px 0' },
  addressText: { margin: '0 0 2px 0', color: '#555', fontSize: '14px' },
  actionRow: { width: '100%', textAlign: 'center', marginBottom: '20px' },
  itemsCard: { background: '#fff', border: '1px solid #ddd', borderRadius: '8px', padding: '16px' },
  itemsList: { marginTop: '12px' },
  itemRow: { display: 'flex', alignItems: 'center', padding: '12px 0', borderBottom: '1px solid #eee' },
  productImg: { width: '70px', height: '70px', objectFit: 'contain', marginRight: '16px' },
  itemDetails: { flex: 1 },
  productName: { margin: '0 0 4px 0', fontSize: '15px', fontWeight: '500' },
  productMeta: { margin: '0 0 2px 0', fontSize: '13px', color: '#666' },
  productPrice: { margin: 0, fontSize: '14px', fontWeight: '600' },
  paymentBtn: { background: '#232f3e', borderRadius: '4px', width: '100%', maxWidth: '400px', padding: '12px 0', fontSize: '16px', fontWeight: '600', color: '#fff', cursor: 'pointer', border: 'none' },
  emptyContainer: { textAlign: 'center', marginTop: '50px' },
  primaryBtn: { background: '#f0c14b', padding: '8px 16px', borderRadius: '4px', cursor: 'pointer' }
};

export default CheckoutDemo;