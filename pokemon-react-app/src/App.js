import React, { useState, useEffect } from 'react';
import './App.css';

function App() {
  const [pokemon, setPokemon] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [editingPokemon, setEditingPokemon] = useState(null);
  const [formData, setFormData] = useState({ name: '', imageUrl: '' });

  useEffect(() => {
    fetchPokemon();
  }, []);

  const fetchPokemon = () => {
    fetch("http://localhost:5243/api/pokemon")
      .then(res => {
        if (!res.ok) {
          throw new Error('Network response was not ok');
        }
        return res.json();
      })
      .then(data => {
        console.log('Data received:', data);
        setPokemon(data);
        setLoading(false);
      })
      .catch(error => {
        console.error('Fetch error:', error);
        setError(error.message);
        setLoading(false);
      });
  };

  const handleAddPokemon = (e) => {
    e.preventDefault();
    
    fetch("http://localhost:5243/api/pokemon", {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: formData.name,
        imageUrl: formData.imageUrl || null
      })
    })
    .then(res => {
      if (!res.ok) {
        throw new Error('Failed to add pokemon');
      }
      return res.json();
    })
    .then(() => {
      fetchPokemon();
      setShowAddForm(false);
      setFormData({ name: '', imageUrl: '' });
    })
    .catch(error => {
      console.error('Add error:', error);
      setError(error.message);
    });
  };

  const handleUpdatePokemon = (e) => {
    e.preventDefault();
    
    fetch(`http://localhost:5243/api/pokemon/${editingPokemon.id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: formData.name,
        imageUrl: formData.imageUrl || null
      })
    })
    .then(res => {
      if (!res.ok) {
        throw new Error('Failed to update pokemon');
      }
      return res.json();
    })
    .then(() => {
      fetchPokemon();
      setEditingPokemon(null);
      setFormData({ name: '', imageUrl: '' });
    })
    .catch(error => {
      console.error('Update error:', error);
      setError(error.message);
    });
  };

  const startEdit = (pokemon) => {
    setEditingPokemon(pokemon);
    setFormData({
      name: pokemon.name,
      imageUrl: pokemon.imageUrl || ''
    });
  };

  const cancelEdit = () => {
    setEditingPokemon(null);
    setFormData({ name: '', imageUrl: '' });
  };

  if (loading) {
    return (
      <div className="App">
        <header className="App-header">
          <h1>Loading Pokemon...</h1>
          <div className="loading-spinner">Loading...</div>
        </header>
      </div>
    );
  }

  if (error) {
    return (
      <div className="App">
        <header className="App-header">
          <h1>Error: {error}</h1>
          <p>Make sure the .NET API is running on http://localhost:5243</p>
          <p>Run: cd PokemonAPI && dotnet run</p>
        </header>
      </div>
    );
  }

  return (
    <div className="App">
      <header className="App-header">
        <h1>Pokemon from .NET Core API</h1>
        
        <button 
          className="add-btn" 
          onClick={() => setShowAddForm(true)}
        >
          Add New Pokemon
        </button>

        <div className="pokemon-container">
          {pokemon.map((p) => (
            <div key={p.id} className="pokemon-card">
              <div className="card-id-corner">#{p.id}</div>
              
              <div className="card-image-section">
                {p.imageUrl && (
                  <img 
                    src={p.imageUrl} 
                    alt={p.name}
                    className="pokemon-image"
                    onError={(e) => {
                      e.target.style.display = 'none';
                    }}
                  />
                )}
              </div>
              
              <div className="card-name-section">
                <div className="pokemon-name">{p.name}</div>
              </div>
              
              <div className="card-footer">
                <button 
                  className="edit-btn"
                  onClick={() => startEdit(p)}
                >
                  Edit
                </button>
              </div>
            </div>
          ))}
        </div>

        {/* Add Pokemon Form */}
        {showAddForm && (
          <div className="modal">
            <div className="modal-content">
              <h2>Add New Pokemon</h2>
              <form onSubmit={handleAddPokemon}>
                <input
                  type="text"
                  placeholder="Pokemon Name"
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})}
                  required
                />
                <input
                  type="text"
                  placeholder="Image URL (optional)"
                  value={formData.imageUrl}
                  onChange={(e) => setFormData({...formData, imageUrl: e.target.value})}
                />
                <div className="form-buttons">
                  <button type="submit">Add Pokemon</button>
                  <button type="button" onClick={() => setShowAddForm(false)}>Cancel</button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Edit Pokemon Form */}
        {editingPokemon && (
          <div className="modal">
            <div className="modal-content">
              <h2>Edit Pokemon</h2>
              <form onSubmit={handleUpdatePokemon}>
                <input
                  type="text"
                  placeholder="Pokemon Name"
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})}
                  required
                />
                <input
                  type="text"
                  placeholder="Image URL (optional)"
                  value={formData.imageUrl}
                  onChange={(e) => setFormData({...formData, imageUrl: e.target.value})}
                />
                <div className="form-buttons">
                  <button type="submit">Update Pokemon</button>
                  <button type="button" onClick={cancelEdit}>Cancel</button>
                </div>
              </form>
            </div>
          </div>
        )}

        </header>
    </div>
  );
}

export default App;
