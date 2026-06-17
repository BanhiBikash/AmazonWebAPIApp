import React, {useState, useEffect} from 'react'

const Banner = () => {
    //Initialized with your specified default banner layout values
    const [banner, setBanner] = useState({
        bannerUrl: "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?q=80&w=1500&h=500&fit=crop&crop=center",
        bannerAlt: "Banner Alt"
    });

    // Keep track of the current active array index tracking
    const [currentSlideIndex, setCurrentSlideIndex] = useState(0);

    const bannerSlides = [
        {
            id: 1,
            image: "https://images.unsplash.com/photo-1531297484001-80022131f5a1?q=80&w=1500&h=500&fit=crop&crop=top",
            alt: "New Tech Vanguard Arrival Deals"
        },
        {
            id: 2,
            image: "https://images.unsplash.com/photo-1441986300917-64674bd600d8?q=80&w=1500&h=500&fit=crop&crop=center",
            alt: "Summer Fashion Collection Sale"
        },
        {
            id: 3,
            image: "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?q=80&w=1500&h=500&fit=crop&crop=center",
            alt: "Mega Flash Clearance Event"
        }
    ];

    // 2. TIMER EFFECT: Cycles through the slides index array positions
    useEffect(() => {
        const timer = setInterval(() => {
            setCurrentSlideIndex((prevIndex) =>
                prevIndex === bannerSlides.length - 1 ? 0 : prevIndex + 1
            );
        }, 4000); // Changes image every 4000ms

        return () => clearInterval(timer); // Clean up memory on unmount
    }, [bannerSlides.length]);

    // Updating the banner object when the active index updates
    useEffect(() => {
        const activeSlide = bannerSlides[currentSlideIndex];
        setBanner({
            bannerUrl: activeSlide.image,
            bannerAlt: activeSlide.alt
        });
    }, [currentSlideIndex]);

    return (
        <div className="hero-banner-slider">
            <img
                src={banner.bannerUrl}
                alt={banner.bannerAlt}
                className="hero-image"
            />
        </div>
    )
}

export default Banner