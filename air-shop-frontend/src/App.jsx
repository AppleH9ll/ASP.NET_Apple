import { useState } from 'react';
import './App.css';

function App() {
    const [login, setLogin] = useState('');
    const [password, setPassword] = useState('');
    const [token, setToken] = useState('');
    const [message, setMessage] = useState('');

    const handleLogin = async (e) => {
        e.preventDefault();
        setMessage('Загрузка...');
        try {
            const response = await fetch('http://localhost:5066/api/ApiAuth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ login, password })
            });

            const data = await response.json();
            if (response.ok) {
                setToken(data.token);
                setMessage(`Успешный вход! Роль: ${data.role}`);
            } else {
                setMessage(`Ошибка: ${data.error}`);
            }
        } catch (error) {
            setMessage(`Ошибка CORS или сети: ${error.message}`);
        }
    };

    const testProtectedEndpoint = async () => {
        try {
            // Делаем запрос к защищенному эндпоинту (например, получение товаров)
            const response = await fetch('http://localhost:5066/api/ProductsApi?page=1&pageSize=2', {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            if (response.ok) {
                const data = await response.json();
                setMessage(`Успешно получено с API: ${data.length} товаров. Первый: ${data[0].name}`);
            } else {
                setMessage(`Отказ в доступе: ${response.status}`);
            }
        } catch (error) {
            setMessage(`Ошибка запроса: ${error.message}`);
        }
    };

    return (
        <div style={{ padding: '40px', fontFamily: 'Arial' }}>
            <h1>AirShop Frontend (React)</h1>
            <div style={{ marginBottom: '20px', padding: '10px', background: '#eee' }}>
                <strong>Сообщение:</strong> {message}
            </div>

            <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', width: '300px', gap: '10px' }}>
                <input type="text" placeholder="Логин" value={login} onChange={e => setLogin(e.target.value)} />
                <input type="password" placeholder="Пароль" value={password} onChange={e => setPassword(e.target.value)} />
                <button type="submit">Войти (получить JWT)</button>
            </form>

            {token && (
                <div style={{ marginTop: '20px' }}>
                    <p style={{ wordBreak: 'break-all', fontSize: '12px', color: 'green' }}>Токен: {token}</p>
                    <button onClick={testProtectedEndpoint} style={{ padding: '10px 20px', background: 'blue', color: 'white' }}>
                        Сделать защищенный запрос (GET /api/ProductsApi)
                    </button>
                </div>
            )}
        </div>
    );
}

export default App;