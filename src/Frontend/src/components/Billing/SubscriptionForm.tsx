import React, { useState } from 'react';

const SubscriptionForm: React.FC = () => {
    const [processId, setProcessId] = useState<string | null>(null);

    const handleSubscribe = async () => {
        // Call backend to get process_id
        // In a real app, you would use a proper API client and handle auth
        try {
            const response = await fetch('http://localhost:5000/billing/subscribe', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId: '3fa85f64-5717-4562-b3fc-2c963f66afa6', plan: 'pro' })
            });
            
            if (response.ok) {
                const data = await response.json();
                setProcessId(data.process_id);
            } else {
                console.error('Failed to start subscription');
            }
        } catch (error) {
            console.error('Error:', error);
        }
    };

    return (
        <div>
            <h2>Subscribe to Pro Plan</h2>
            {!processId ? (
                <button onClick={handleSubscribe}>Subscribe</button>
            ) : (
                <div id="bancard-iframe-container">
                    {/* Bancard iframe would be loaded here using the processId */}
                    <p>Loading Bancard iframe for process: {processId}</p>
                    {/* Note: The actual URL depends on the environment (staging vs production) */}
                    <iframe src={`https://vpos.infonet.com.py/checkout/register_card?process_id=${processId}`} 
                            width="100%" height="400px" title="Bancard"></iframe>
                </div>
            )}
        </div>
    );
};

export default SubscriptionForm;
