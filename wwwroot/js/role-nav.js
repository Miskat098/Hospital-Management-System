// role-nav.js - small helper used by role-specific static pages
async function getCurrentUser(){
  try{
    const res = await fetch('/api/account/current');
    if(!res.ok) return { authenticated: false };
    const j = await res.json();
    return j;
  }catch(e){ return { authenticated: false }; }
}

// export in browsers
window.getCurrentUser = getCurrentUser;
