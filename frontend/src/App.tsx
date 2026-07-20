import { useEffect, useState } from 'react';
import axios from 'axios';

type AuthMode = 'login' | 'register';

type Workspace = {
  id: string;
  name: string;
  description: string;
};

type WorkspaceDocument = {
  id: string;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  uploadedAtUtc: string;
};

function App() {
  const [mode, setMode] = useState<AuthMode>('login');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [workspaceName, setWorkspaceName] = useState('');
  const [workspaceDescription, setWorkspaceDescription] = useState('');
  const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
  const [workspaceDocuments, setWorkspaceDocuments] = useState<WorkspaceDocument[]>([]);
  const [token, setToken] = useState('');
  const [workspaceId, setWorkspaceId] = useState('');
  const [question, setQuestion] = useState('');
  const [answer, setAnswer] = useState('');
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState<'success' | 'error' | 'info'>('info');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const isAuthenticated = Boolean(token);
  const isWorkspaceCreated = Boolean(workspaceId);

  const getAuthHeaders = () => ({
    ...(token ? { Authorization: `Bearer ${token}` } : {})
  });

  const showMessage = (text: string, type: 'success' | 'error' | 'info' = 'info') => {
    setMessage(text);
    setMessageType(type);
  };

  const loadWorkspaceDocuments = async (workspaceIdToLoad: string, authToken?: string) => {
    const tokenToUse = authToken ?? token;
    if (!tokenToUse || !workspaceIdToLoad) {
      return;
    }

    try {
      const response = await axios.get(`/api/workspaces/${workspaceIdToLoad}/documents`, {
        headers: {
          Authorization: `Bearer ${tokenToUse}`
        }
      });

      setWorkspaceDocuments(response.data);
    } catch {
      setWorkspaceDocuments([]);
    }
  };

  const loadWorkspaces = async (authToken?: string, preferredWorkspaceId?: string) => {
    const tokenToUse = authToken ?? token;
    if (!tokenToUse) {
      return;
    }

    try {
      const response = await axios.get('/api/workspaces', {
        headers: {
          Authorization: `Bearer ${tokenToUse}`
        }
      });

      setWorkspaces(response.data);

      const savedWorkspaceId = preferredWorkspaceId ?? localStorage.getItem('workspaceId') ?? '';
      const activeWorkspaceId = response.data.some((workspace: Workspace) => workspace.id === savedWorkspaceId)
        ? savedWorkspaceId
        : response.data.length > 0
          ? response.data[0].id
          : '';

      if (activeWorkspaceId) {
        setWorkspaceId(activeWorkspaceId);
        localStorage.setItem('workspaceId', activeWorkspaceId);
        await loadWorkspaceDocuments(activeWorkspaceId, tokenToUse);
      } else {
        setWorkspaceId('');
        localStorage.removeItem('workspaceId');
        setWorkspaceDocuments([]);
      }
    } catch {
      // ignore load errors for now
    }
  };

  const handleLogout = () => {
    setToken('');
    setWorkspaceId('');
    setWorkspaces([]);
    setWorkspaceDocuments([]);
    localStorage.removeItem('token');
    localStorage.removeItem('workspaceId');
    showMessage('Signed out successfully.', 'success');
  };

  const handleWorkspaceSelect = async (id: string) => {
    setWorkspaceId(id);
    localStorage.setItem('workspaceId', id);
    await loadWorkspaceDocuments(id);
  };

  useEffect(() => {
    const savedToken = localStorage.getItem('token') ?? '';
    const savedWorkspaceId = localStorage.getItem('workspaceId') ?? '';

    if (savedToken) {
      setToken(savedToken);
      loadWorkspaces(savedToken, savedWorkspaceId);
    }
  }, []);

  const handleAuth = async () => {
    try {
      const endpoint = mode === 'login' ? '/api/auth/login' : '/api/auth/register';
      const payload = mode === 'login'
        ? { email, password }
        : { firstName, lastName, email, password };

      const response = await axios.post(endpoint, payload);
      const authToken = response.data.token;
      setToken(authToken);
      localStorage.setItem('token', authToken);
      await loadWorkspaces(authToken);
      showMessage(`${mode === 'login' ? 'Signed in' : 'Registered'} successfully.`, 'success');
    } catch {
      showMessage('Authentication failed. Please check your credentials.', 'error');
    }
  };

  const createWorkspace = async () => {
    if (!workspaceName.trim()) {
      showMessage('Workspace name cannot be empty.', 'error');
      return;
    }

    try {
      const response = await axios.post('/api/workspaces', {
        name: workspaceName,
        description: workspaceDescription
      }, {
        headers: getAuthHeaders()
      });

      setWorkspaceId(response.data.id);
      localStorage.setItem('workspaceId', response.data.id);
      await loadWorkspaces();
      showMessage('Workspace created successfully.', 'success');
    } catch {
      showMessage('Unable to create workspace. Please try again.', 'error');
    }
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null;
    setSelectedFile(file);

    if (file) {
      showMessage(`Ready to upload ${file.name}`, 'info');
    }
  };

  const uploadDocument = async () => {
    if (!isWorkspaceCreated) {
      showMessage('Create a workspace before uploading documents.', 'error');
      return;
    }

    if (!selectedFile) {
      showMessage('Select a document file first.', 'error');
      return;
    }

    const formData = new FormData();
    formData.append('workspaceId', workspaceId);
    formData.append('file', selectedFile);

    try {
      await axios.post('/api/documents/upload', formData, {
        headers: getAuthHeaders()
      });

      setSelectedFile(null);
      await loadWorkspaceDocuments(workspaceId);
      showMessage('Document uploaded successfully.', 'success');
    } catch {
      showMessage('Unable to upload the selected document.', 'error');
    }
  };

  const deleteWorkspace = async (id: string) => {
    try {
      await axios.delete(`/api/workspaces/${id}`, {
        headers: getAuthHeaders()
      });

      await loadWorkspaces();
      showMessage('Workspace deleted successfully.', 'success');
    } catch {
      showMessage('Unable to delete workspace. Please try again.', 'error');
    }
  };

  const deleteDocument = async (documentId: string) => {
    if (!isWorkspaceCreated) {
      showMessage('Select a workspace before deleting documents.', 'error');
      return;
    }

    try {
      await axios.delete(`/api/workspaces/${workspaceId}/documents/${documentId}`, {
        headers: getAuthHeaders()
      });

      await loadWorkspaceDocuments(workspaceId);
      showMessage('Document deleted successfully.', 'success');
    } catch {
      showMessage('Unable to delete document. Please try again.', 'error');
    }
  };

  const askQuestion = async () => {
    if (!isWorkspaceCreated) {
      showMessage('Select or create a workspace before asking questions.', 'error');
      return;
    }

    if (!question.trim()) {
      showMessage('Enter a question before sending.', 'error');
      return;
    }

    try {
      const response = await axios.post('/api/chat/ask', {
        workspaceId,
        question
      }, {
        headers: getAuthHeaders()
      });

      setAnswer(response.data.answer);
      showMessage('Answer received.', 'success');
    } catch {
      showMessage('Unable to ask question. Please try again.', 'error');
    }
  };

  return (
    <div className="app-shell">
      <header className="app-header">
        <div>
          <p className="eyebrow">Ai Doc Reader</p>
          <h1>Ai Doc Reader</h1>
          <p className="hero-copy">
            Upload documents, create workspaces, and ask AI-powered questions from your content.
          </p>
        </div>

        <div className="status-panel">
          <div className={`status-pill ${isAuthenticated ? 'pill-success' : 'pill-muted'}`}>
            {isAuthenticated ? 'Signed in' : 'Not signed in'}
          </div>
          <div className={`status-pill ${isWorkspaceCreated ? 'pill-success' : 'pill-muted'}`}>
            {isWorkspaceCreated ? 'Workspace ready' : 'No workspace yet'}
          </div>
          {isWorkspaceCreated && <div className="chip">ID: {workspaceId}</div>}
        </div>
      </header>

      <div className="dashboard-grid">
        {!isAuthenticated ? (
          <section className="card card-primary">
            <h2>{mode === 'login' ? 'Login to your account' : 'Register a new account'}</h2>

            <div className="form-grid">
              {mode === 'register' && (
                <>
                  <input placeholder="First name" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
                  <input placeholder="Last name" value={lastName} onChange={(e) => setLastName(e.target.value)} />
                </>
              )}

              <input placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
              <input type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} />
              <button className="primary-button" onClick={handleAuth}>{mode === 'login' ? 'Login' : 'Register'}</button>
            </div>

            <div className="auth-toggle-link">
              {mode === 'login' ? (
                <p>
                  Don&apos;t have an account? <button type="button" className="link-button" onClick={() => setMode('register')}>Register</button>
                </p>
              ) : (
                <p>
                  Already have an account? <button type="button" className="link-button" onClick={() => setMode('login')}>Login</button>
                </p>
              )}
            </div>
          </section>
        ) : (
          <>
            <section className="card card-primary">
              <h2>Your workspaces</h2>
              <p>Select a workspace to view or upload documents.</p>
              {workspaces.length === 0 ? (
                <p className="empty-state">No saved workspaces yet. Create one to get started.</p>
              ) : (
                <div className="workspace-list">
                  {workspaces.map((workspace) => (
                    <div
                      key={workspace.id}
                      className={`workspace-card ${workspace.id === workspaceId ? 'active' : ''}`}
                      onClick={() => handleWorkspaceSelect(workspace.id)}
                    >
                      <div className="workspace-content">
                        <strong>{workspace.name}</strong>
                        <span>{workspace.description}</span>
                      </div>
                      <button
                        type="button"
                        className="danger-button workspace-delete-button"
                        onClick={(e) => {
                          e.stopPropagation();
                          deleteWorkspace(workspace.id);
                        }}
                      >
                        Delete
                      </button>
                    </div>
                  ))}
                </div>
              )}
              <button className="secondary-button logout-button" onClick={handleLogout}>Sign out</button>
            </section>

            <section className="card card-secondary">
              <h3>Workspace files</h3>
              <p>See uploaded files for the selected workspace, and remove any file you no longer need.</p>
              {workspaceDocuments.length === 0 ? (
                <p className="empty-state">No documents uploaded for this workspace yet.</p>
              ) : (
                <ul className="document-list">
                  {workspaceDocuments.map((document) => (
                    <li key={document.id} className="document-item">
                      <div>
                        <strong>{document.fileName}</strong>
                        <span>{document.contentType} · {Math.round(document.fileSize / 1024)} KB</span>
                        <span>Uploaded {new Date(document.uploadedAtUtc).toLocaleString()}</span>
                      </div>
                      <button className="danger-button" type="button" onClick={() => deleteDocument(document.id)}>
                        Delete
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </section>

            <section className="card card-secondary">
              <h3>Workspace builder</h3>
              <p>Keep your documents and questions grouped for each team or project.</p>
              <input placeholder="Workspace name" value={workspaceName} onChange={(e) => setWorkspaceName(e.target.value)} />
              <input placeholder="Workspace description" value={workspaceDescription} onChange={(e) => setWorkspaceDescription(e.target.value)} />
              <button className="secondary-button" onClick={createWorkspace}>Create workspace</button>
            </section>

            <section className="card card-secondary">
              <h3>Upload documents</h3>
              <p>Choose the file you want to ingest into the workspace.</p>
              <input type="file" onChange={handleFileChange} accept=".pdf,.docx,.txt" />
              {selectedFile && (
                <div className="file-summary">
                  <span>{selectedFile.name}</span>
                  <span>{Math.round(selectedFile.size / 1024)} KB</span>
                </div>
              )}
              <button className="secondary-button" onClick={uploadDocument} disabled={!isWorkspaceCreated}>Upload document</button>
            </section>

            <section className="card card-chat">
              <h3>Ask the workspace</h3>
              <p>Send a question and receive an answer generated from document content.</p>
              <textarea placeholder="Ask about your uploaded documents..." value={question} onChange={(e) => setQuestion(e.target.value)} />
              <button className="primary-button" onClick={askQuestion} disabled={!isWorkspaceCreated}>Get answer</button>

              {answer && (
                <div className="response-panel">
                  <h4>Answer</h4>
                  <pre>{answer}</pre>
                </div>
              )}
            </section>
          </>
        )}
      </div>

      {message && (
        <div className={`toast ${messageType === 'success' ? 'toast-success' : messageType === 'error' ? 'toast-error' : 'toast-info'}`}>
          {message}
        </div>
      )}
    </div>
  );
}

export default App;
