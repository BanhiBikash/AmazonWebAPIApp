import React, { useState, useContext, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import UserContext from '../context/UserContext';
import api from '../api/axiosConfig';

const Account_Update = () => {
    const { user, setUser } = useContext(UserContext);
    const navigate = useNavigate();

    // Toggle state to manage read-only info vs active form view
    const [isEditing, setIsEditing] = useState(false);

    // State for form tracking
    const [profileData, setProfileData] = useState({
        address: '',
        city: '',
        state: '',
        postalCode: '',
        country: ''
    });

    const [selectedFile, setSelectedFile] = useState(null);
    const [previewUrl, setPreviewUrl] = useState('');
    const [uiStatus, setUiStatus] = useState({ loading: false, success: null, error: null });

    // Reusable helper to seed inputs with existing database details
    const syncFormWithContext = (userData) => {
        if (userData) {
            setProfileData({
                address: userData.address || '',
                city: userData.city || '',
                state: userData.state || '',
                postalCode: userData.postalCode || '',
                country: userData.country || ''
            });
            if (userData.profileImageUrl) {
                setPreviewUrl(userData.profileImageUrl);
            }
        }
    };

    // 🎯 FETCH FRESH PROFILE AND ADDRESS DATA WHEN COMPONENT MOUNTS
    useEffect(() => {
        const fetchCurrentProfile = async () => {
            try {
                setUiStatus({ loading: true, success: null, error: null });

                const response = await api.get('/v1/Account/getprofiledetails', {
                    headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                    }
                });

                // Seed current details cleanly into the layout
                syncFormWithContext(response.data);

                setUser(prev => ({
                    ...prev,
                    email: response.data.email || prev?.email,
                    name: response.data.firstName && response.data.lastName
                        ? `${response.data.firstName} ${response.data.lastName}`
                        : prev?.name,
                    address: response.data.address,
                    city: response.data.city,
                    state: response.data.state,
                    postalCode: response.data.postalCode,
                    country: response.data.country,
                    profileImageUrl: response.data.profileImageUrl
                }));

                setUiStatus({ loading: false, success: null, error: null });
            } catch (err) {
                console.error("Profile initialization fetch error:", err);
                const errorMessage = err.response?.data?.message || err.message || 'Could not securely fetch profile details.';
                setUiStatus({ loading: false, success: null, error: errorMessage });
            }
        };

        fetchCurrentProfile();
    }, [setUser]);

    // Route fallback guard for unauthenticated instances
    if (!user) {
        return (
            <div className="account-dashboard-fallback">
                <h2>Please log in to alter security profiles.</h2>
                <Link to="/login" className="auth-action-btn-gold" style={{ padding: '8px 24px' }}>Sign In</Link>
            </div>
        );
    }

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setProfileData(prev => ({ ...prev, [name]: value }));
    };

    const handleFileChange = (e) => {
        const file = e.target.files[0];
        if (file) {
            setSelectedFile(file);
            setPreviewUrl(URL.createObjectURL(file));
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setUiStatus({ loading: true, success: null, error: null });

        // Pack multipart payload fields
        const multiPartForm = new FormData();
        multiPartForm.append('address', profileData.address || '');
        multiPartForm.append('city', profileData.city || '');
        multiPartForm.append('state', profileData.state || '');
        multiPartForm.append('postalCode', profileData.postalCode || '');
        multiPartForm.append('country', profileData.country || '');

        // Explicitly append an empty entry if no file is uploaded 
        if (selectedFile) {
            multiPartForm.append('profileImage', selectedFile);
        } else {
            multiPartForm.append('profileImage', '');
        }

        try {
            const response = await api.put('/v1/Account/UpdateProfile', multiPartForm, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            });

            setUiStatus({
                loading: false,
                success: 'Your personal delivery records have been saved successfully!',
                error: null
            });

            // Synchronize backend response changes with global React tracking context
            setUser(prev => ({
                ...prev,
                address: response.data.address,
                city: response.data.city,
                state: response.data.state,
                postalCode: response.data.postalCode,
                country: response.data.country,
                profileImageUrl: response.data.profileImageUrl
            }));

            // Set fresh fields as default for subsequent updates
            syncFormWithContext(response.data);
            setSelectedFile(null);

            // Exit out of editing panel view automatically on success
            setIsEditing(false);

        } catch (err) {
            console.error(err);
            const errorMessage = err.response?.data?.message || err.message || 'Profile alteration communication failure.';
            setUiStatus({ loading: false, success: null, error: errorMessage });
        }
    };

    return (
        <div className="auth-page-container">
            <div className="auth-logo-header">
                <Link to="/account" className="account-back-breadcrumb">
                    ‹ Back to Dashboard
                </Link>
            </div>

            <div className="auth-card-box register-card-wide">
                <h2 className="auth-card-title">Login & Security</h2>

                {uiStatus.success && <div className="admin-status-alert success">{uiStatus.success}</div>}
                {uiStatus.error && <div className="admin-status-alert error">{uiStatus.error}</div>}
                {uiStatus.loading && !isEditing && <div style={{ fontSize: '0.85rem', color: '#565959', marginBottom: '10px' }}>Loading account profile records...</div>}

                {/* 📋 VIEW 1: READ-ONLY INFORMATION LAYER */}
                {!isEditing ? (
                    <div className="auth-form-flow">
                        <p className="profile-subtitle-context">Review your account footprint and current settings parameters below.</p>

                        <div className="profile-avatar-management-node" style={{ backgroundColor: '#ffffff', justifyContent: 'center' }}>
                            <div className="profile-avatar-preview-shell" style={{ width: '90px', height: '90px' }}>
                                {user.profileImageUrl ? (
                                    <img src={user.profileImageUrl} alt="Current Avatar" className="profile-avatar-circle" />
                                ) : (
                                    <div className="profile-avatar-placeholder" style={{ fontSize: '2.5rem' }}>👤</div>
                                )}
                            </div>
                        </div>

                        <div className="account-read-only-row" style={{ borderBottom: '1px solid #e7e7e7', paddingBottom: '12px' }}>
                            <div style={{ fontSize: '0.85rem', fontWeight: '700', color: '#0f1111' }}>Name:</div>
                            <div style={{ fontSize: '0.9rem', color: '#333', marginTop: '2px' }}>{user.name || user.userName || 'Not Provided'}</div>
                        </div>

                        <div className="account-read-only-row" style={{ borderBottom: '1px solid #e7e7e7', paddingBottom: '12px', marginTop: '8px' }}>
                            <div style={{ fontSize: '0.85rem', fontWeight: '700', color: '#0f1111' }}>Email Address:</div>
                            <div style={{ fontSize: '0.9rem', color: '#333', marginTop: '2px' }}>{user.email || 'Not Provided'}</div>
                        </div>

                        <div className="account-read-only-row" style={{ borderBottom: '1px solid #e7e7e7', paddingBottom: '12px', marginTop: '8px' }}>
                            <div style={{ fontSize: '0.85rem', fontWeight: '700', color: '#0f1111' }}>Shipping Address Parameters:</div>
                            {user.address ? (
                                <div style={{ fontSize: '0.9rem', color: '#333', marginTop: '4px', lineHeight: '1.4' }}>
                                    {user.address}<br />
                                    {user.city}{user.state ? `, ${user.state}` : ''} {user.postalCode}<br />
                                    {user.country}
                                </div>
                            ) : (
                                <div style={{ fontSize: '0.85rem', color: '#767676', fontStyle: 'italic', marginTop: '4px' }}>No physical address records on file.</div>
                            )}
                        </div>

                        <button
                            type="button"
                            className="auth-secondary-create-btn"
                            onClick={() => {
                                syncFormWithContext(user); // 🎯 Hydrates the form variables directly with actual context updates
                                setIsEditing(true);
                                setUiStatus(prev => ({ ...prev, success: null, error: null }));
                            }}
                            style={{ marginTop: '15px', fontWeight: '500' }}
                            disabled={uiStatus.loading}
                        >
                            Edit Profile Information
                        </button>
                    </div>
                ) : (

                    /* ✍️ VIEW 2: INTERACTIVE ALTERATION MULTIPART FORM LAYER */
                    <form onSubmit={handleSubmit} className="auth-form-flow">
                        <p className="profile-subtitle-context">Update your global shipping vectors and identity footprint below.</p>

                        {/* Avatar Graphic Segment */}
                        <div className="profile-avatar-management-node">
                            <div className="profile-avatar-preview-shell">
                                {previewUrl ? (
                                    <img
                                        /* 🎯 FIX: If it's a freshly chosen local file blob, use it directly. Otherwise, point to the backend host. */
                                        src={previewUrl.startsWith('blob:') ? previewUrl : previewUrl}
                                        alt="Profile Image"
                                        className="profile-avatar-circle"
                                    />
                                ) : (
                                    <div className="profile-avatar-placeholder">👤</div>
                                )}
                            </div>
                            <div className="auth-input-group" style={{ flex: 1 }}>
                                <label htmlFor="avatar-upload">Profile Photo</label>
                                <input
                                    id="avatar-upload"
                                    type="file"
                                    accept="image/*"
                                    onChange={handleFileChange}
                                />
                            </div>
                        </div>

                        <div className="auth-input-group">
                            <label>Street Address</label>
                            <input
                                type="text"
                                name="address"
                                value={profileData.address}
                                onChange={handleInputChange}
                                placeholder="123 Amazon Way, Apt 4B"
                            />
                        </div>

                        <div className="auth-form-row-grid">
                            <div className="auth-input-group">
                                <label>City</label>
                                <input
                                    type="text"
                                    name="city"
                                    value={profileData.city}
                                    onChange={handleInputChange}
                                />
                            </div>
                            <div className="auth-input-group">
                                <label>State / Region</label>
                                <input
                                    type="text"
                                    name="state"
                                    value={profileData.state}
                                    onChange={handleInputChange}
                                />
                            </div>
                        </div>

                        <div className="auth-form-row-grid">
                            <div className="auth-input-group">
                                <label>Postal Code</label>
                                <input
                                    type="text"
                                    name="postalCode"
                                    value={profileData.postalCode}
                                    onChange={handleInputChange}
                                />
                            </div>
                            <div className="auth-input-group">
                                <label>Country</label>
                                <input
                                    type="text"
                                    name="country"
                                    value={profileData.country}
                                    onChange={handleInputChange}
                                />
                            </div>
                        </div>

                        <div style={{ display: 'flex', gap: '10px', marginTop: '12px' }}>
                            <button
                                type="button"
                                className="auth-secondary-create-btn"
                                onClick={() => {
                                    syncFormWithContext(user); // Reset any unsaved changes on cancel click
                                    setIsEditing(false);
                                }}
                                disabled={uiStatus.loading}
                                style={{ flex: 1, margin: 0 }}
                            >
                                Cancel
                            </button>

                            <button
                                type="submit"
                                className="auth-action-btn-gold"
                                disabled={uiStatus.loading}
                                style={{ flex: 1, margin: 0 }}
                            >
                                {uiStatus.loading ? 'Saving...' : 'Save Changes'}
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
};

export default Account_Update;