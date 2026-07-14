import { useState } from 'react';
import axios from 'axios';

type AuthMode = 'login' | 'register';

function App() {
  const [mode, setMode] = useState<AuthMode>('login');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [workspaceName, setWorkspaceName] = useState('');
  const [workspaceDescription, setWorkspaceDescription] = useState('');
  const [token, setToken] = useState('');
  const [workspaceId, setWorkspaceId] = useState('');
  const [question, setQuestion] = useState('');
  const [answer, setAnswer] = useState('');
  const [message, setMessage] = useState('');

  const getAuthHeaders = () => ({
    ...(token ? { Authorization: `Bearer ${token}` } : {})
  });

  const handleAuth = async () => {
    try {
      const endpoint = mode === 'login' ? '/api/auth/login' : '/api/auth/register';
      const payload = mode === 'login'
        ? { email, password }
        : { firstName, lastName, email, password };

      const response = await axios.post(endpoint, payload);
      setToken(response.data.token);
      setMessage(`${mode === 'login' ? 'Signed in' : 'Registered'} successfully.`);
    } catch (error) {
      setMessage('Authentication failed. Please try again.');
    }
  };

  const createWorkspace = async () => {
    try {
      const response = await axios.post('/api/workspaces', {
        name: workspaceName,
        description: workspaceDescription
      }, {
        headers: getAuthHeaders()
      });

      setWorkspaceId(response.data.id);
      setMessage('Workspace created successfully.');
    } catch {
      setMessage('Unable to create workspace.');
    }
  };

  const uploadDocument = async () => {
    try {
      await axios.post('/api/documents/upload', {
        workspaceId,
        fileName: 'sample-document.pdf',
        contentType: 'application/pdf',
        fileSize: 1200,
        storagePath: '/tmp/sample-document.pdf'
      }, {
        headers: getAuthHeaders()
      });

      setMessage('Document metadata uploaded successfully.');
    } catch {
      setMessage('Unable to upload document metadata.');
    }
  };

  const askQuestion = async () => {
    try {
      const response = await axios.post('/api/chat/ask', {
        workspaceId,
        question
      }, {
        headers: getAuthHeaders()
      });

      setAnswer(response.data.answer);
      setMessage('Question answered.');
    } catch {
      setMessage('Unable to ask question.');
    }
  };

  return (
    <div className="app-shell">
      <header>
        <h1>Enterprise Knowledge Assistant</h1>
        <p>Search and ask questions across your company documents.</p>
      </header>

      <section className="card">
        <div className="toggle-row">
          <button className={mode === 'login' ? 'active' : ''} onClick={() => setMode('login')}>Login</button>
          <button className={mode === 'register' ? 'active' : ''} onClick={() => setMode('register')}>Register</button>
        </div>

        {mode === 'register' && (
          <>
            <input placeholder="First name" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
            <input placeholder="Last name" value={lastName} onChange={(e) => setLastName(e.target.value)} />
          </>
        )}

        <input placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
        <input type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} />
        <button onClick={handleAuth}>{mode === 'login' ? 'Sign in' : 'Create account'}</button>
      </section>

      <section className="card">
        <h2>Create workspace</h2>
        <input placeholder="Workspace name" value={workspaceName} onChange={(e) => setWorkspaceName(e.target.value)} />
        <input placeholder="Workspace description" value={workspaceDescription} onChange={(e) => setWorkspaceDescription(e.target.value)} />
        <button onClick={createWorkspace}>Create</button>
      </section>

      <section className="card">
        <h2>Upload document</h2>
        <p>Simulate document ingestion for a workspace.</p>
        <button onClick={uploadDocument}>Upload sample document</button>
      </section>

      <section className="card">
        <h2>Ask a question</h2>
        <textarea placeholder="Ask about your documents" value={question} onChange={(e) => setQuestion(e.target.value)} />
        <button onClick={askQuestion}>Ask</button>
        {answer && <pre>{answer}</pre>}
      </section>

      {message && <p className="message">{message}</p>}
    </div>
  );
}

export default App;
