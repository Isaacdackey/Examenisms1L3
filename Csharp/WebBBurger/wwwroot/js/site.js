// Brasil Burger - site.js premium micro-interactions
(function(){
  // Navbar scroll compact
  const navbar = document.querySelector(".navbar");
  if (navbar) {
    const onScroll = () => {
      if (window.scrollY > 20) navbar.classList.add("scrolled");
      else navbar.classList.remove("scrolled");
    };
    window.addEventListener("scroll", onScroll, {passive:true});
    onScroll();
  }

  // Reveal on scroll (fade-in)
  const reveals = document.querySelectorAll(".reveal");
  if ("IntersectionObserver" in window && reveals.length) {
    const io = new IntersectionObserver((entries)=>{
      entries.forEach(e=>{
        if(e.isIntersecting){ e.target.classList.add("visible"); io.unobserve(e.target); }
      });
    }, {threshold:0.12});
    reveals.forEach(el=>io.observe(el));
  } else {
    reveals.forEach(el=>el.classList.add("visible"));
  }

  // Auto-dismiss ONLY toast alerts, not inline info alerts
  const toastAlerts = document.querySelectorAll(".toast-stack .alert");
  toastAlerts.forEach(a=>{
    setTimeout(()=> {
      a.style.transition = "all 0.4s ease";
      a.style.opacity = "0";
      a.style.transform = "translateY(-8px)";
      setTimeout(()=> a.remove(), 420);
    }, 5000);
  });

  // Payment option selection visual - scoped by radio name
  document.querySelectorAll(".payment-option input[type=\"radio\"]").forEach(r=>{
    r.addEventListener("change", ()=>{
      const group = r.name;
      document.querySelectorAll(".payment-option input[name=\"" + group + "\"]").forEach(inp=>{
        inp.closest(".payment-option").classList.remove("selected");
      });
      const card = r.closest(".payment-option");
      if(card) card.classList.add("selected");
    });
    if(r.checked) r.dispatchEvent(new Event("change"));
  });

  // Quantity helpers (delta buttons)
  window.changeQuantity = window.changeQuantity || function(delta){
    const inp = document.getElementById("quantity");
    const inp2 = document.getElementById("quantityInput");
    const target = inp || inp2;
    if(!target) return;
    let v = parseInt(target.value)||1;
    v = Math.min(10, Math.max(1, v+delta));
    target.value = v;
    target.dispatchEvent(new Event("change"));
  };

  // Add to cart feedback - small pulse on button
  document.querySelectorAll("form[asp-action=\"Add\"] button, form[action*=\"Cart\"] button[type=\"submit\"]").forEach(btn=>{
    btn.addEventListener("click", function(){
      this.style.transform = "scale(0.97)";
      setTimeout(()=> this.style.transform = "", 180);
    });
  });

  // Search input clear on Escape
  const search = document.querySelector(".search-box input, input[name=\"search\"]");
  if(search){
    search.addEventListener("keydown", e=>{ if(e.key==="Escape"){ e.target.value=""; e.target.blur(); }});
  }
})();
