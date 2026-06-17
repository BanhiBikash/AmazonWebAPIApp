import React from 'react';

const WarningDialog = ({ isOpen, message, onClose }) => {
  if (!isOpen) return null;

  return (
    <div style={styles.overlay}>
      <div style={styles.modalBox}>
        {/* Warning Icon and Header */}
        <div style={styles.header}>
          <span style={styles.icon}>⚠️</span>
          <h3 style={styles.title}>Attention Needed</h3>
        </div>
        
        {/* Dynamic Alert Message Body */}
        <div style={styles.body}>
          <p style={styles.text}>{message}</p>
        </div>

        {/* Sole Action Dismiss Button */}
        <div style={styles.footer}>
          <button style={styles.btn} onClick={onClose}>
            OK
          </button>
        </div>
      </div>
    </div>
  );
};

// Clean Inline Styles for self-contained isolation & strict screen blocking
const styles = {
  overlay: {
    position: 'fixed',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.6)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 99999, // Guarantees overlay sit above navigation layers
    backdropFilter: 'blur(2px)',
  },
  modalBox: {
    backgroundColor: '#ffffff',
    width: '90%',
    maxWidth: '420px',
    borderRadius: '8px',
    padding: '24px',
    boxShadow: '0 4px 20px rgba(0, 0, 0, 0.15)',
    animation: 'fadeIn 0.2s ease-out',
  },
  header: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    marginBottom: '16px',
    borderBottom: '1px solid #f0f0f0',
    paddingBottom: '12px',
  },
  icon: {
    fontSize: '1.6rem',
  },
  title: {
    margin: 0,
    fontSize: '1.25rem',
    color: '#333333',
    fontWeight: '600',
  },
  body: {
    marginBottom: '24px',
  },
  text: {
    margin: 0,
    color: '#555555',
    lineHeight: '1.5',
    fontSize: '0.95rem',
  },
  footer: {
    display: 'flex',
    justifyContent: 'flex-end',
  },
  btn: {
    backgroundColor: '#ffd814',
    border: '1px solid #fcd200',
    borderRadius: '4px',
    padding: '8px 24px',
    fontSize: '0.9rem',
    fontWeight: '500',
    cursor: 'pointer',
    boxShadow: '0 2px 5px rgba(213,217,217,.5)',
    transition: 'background-color 0.1s',
  },
};

export default WarningDialog;