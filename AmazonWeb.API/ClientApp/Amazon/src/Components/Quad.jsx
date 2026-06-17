import React from 'react'
import { Link,useNavigate } from 'react-router-dom'; 

const Quad = (props) => {

    const itemsArray = props.items;
    const referUrl = props.referTo;
    const header = props.topic;

    //hook
    const navigate = useNavigate();

  return (
          
        <div className="product-card-container">
          <h2 className="card-title">{header}</h2>

          {/* go through first 4 items and display them */}
          <div className="quad-image-grid">
            {Array.isArray(itemsArray) && itemsArray.slice(0, 4).map((item) => {
              // Optional: strips out database prefix underscores if present (e.g., "Decor_Mirrors" -> "Mirrors")
              const displaySubCategory = item.subCategory && item.subCategory.includes('_')
                ? item.subCategory.split('_')[1]
                : item.subCategory;

              return (
                <div className="quad-item" key={item.id || item.productId}>
                  <img
                    src={item.imageUrl}
                    alt={item.name || "Home Decor"}
                    onError={(e) => {
                      e.target.onerror = null;
                      e.target.src = 'https://placehold.co/300?text=Image+Load+Error';
                    }}
                    onClick={function(){navigate(`../product/${item.id}`)}}
                  />
                  <span>{displaySubCategory || 'View Item'}</span>
                </div>
              );
            })}
          </div>
          <Link to={referUrl} className="card-explore-link">Check out</Link>
        </div>
  )
}

export default Quad