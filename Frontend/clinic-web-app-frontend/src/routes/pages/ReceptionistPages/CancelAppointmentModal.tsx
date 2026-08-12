interface CancelAppointmentModalProps {
    isOpen: boolean;
    reason: string;
    reasonError?: string | null;
    isCancelling?: boolean;
    onReasonChange: (reason: string) => void;
    onConfirm: () => void;
    onDismiss: () => void;
}

export function CancelAppointmentModal({
                                           isOpen,
                                           reason,
                                           reasonError,
                                           isCancelling = false,
                                           onReasonChange,
                                           onConfirm,
                                           onDismiss,
                                       }: CancelAppointmentModalProps) {
    if (!isOpen) return null;

    return (
        <div className="modal-overlay">
            <div className="modal-content">
                <p className="modal-title">
                    Do you really want to cancel the appointment? It will be permanently deleted.
                </p>

                <div className="modal-field">
                    <label>Reason for cancellation:</label>
                    <textarea
                        placeholder="Enter reason (e.g. Client requested cancellation)..."
                        value={reason}
                        onChange={(e) => onReasonChange(e.target.value)}
                    />
                    {reasonError && (
                        <span style={{ color: "#ef4444", fontSize: "12px", marginTop: "2px" }}>
                            {reasonError}
                        </span>
                    )}
                </div>

                <div className="modal-actions">
                    <button
                        className="btn btn-secondary"
                        onClick={onDismiss}
                        disabled={isCancelling}
                    >
                        No
                    </button>
                    <button
                        className="btn btn-decline"
                        onClick={onConfirm}
                        disabled={isCancelling}
                    >
                        {isCancelling ? "Cancelling..." : "Yes"}
                    </button>
                </div>
            </div>
        </div>
    );
}