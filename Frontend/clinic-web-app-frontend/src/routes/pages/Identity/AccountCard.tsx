import {useState} from "react";
import {profilesApi} from "../../../services/api/ProfilesApi.ts";

interface AccountCardProps {
    email: string;
    phoneNumber: string;
    firstName: string;
    lastName: string;
    middleName?: string;
}

export function AccountCard({ email, phoneNumber, firstName, lastName, middleName }: AccountCardProps) {
    return (
        <div style={{ display: "flex", flexDirection: "row", gap: "16px", padding: "20px", background: "#222", color: "#fff", borderRadius: "8px" }}>
            <p><strong>Name:</strong> {lastName} {firstName} {middleName}</p>
            <p><strong>Email:</strong> {email}</p>
            <p><strong>Phone:</strong> {phoneNumber}</p>
        </div>
    );
}

interface AccountCreateCardProps {
    onSuccess?: () => void;
}
export function AccountCreateCard({ onSuccess }: AccountCreateCardProps) {
    const [isCreating, setIsCreating] = useState(false);
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [middleName, setMiddleName] = useState("");
    const [phoneNumber, setPhoneNumber] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);

    const handleSubmit = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        setError(null);
        setSuccess(false);

        if (!firstName.trim() || !lastName.trim()) {
            setError("First name and last name are required.");
            return;
        }

        setIsCreating(true);
        try {
            const result = await profilesApi.createAccountMe({
                firstName: firstName.trim(),
                lastName: lastName.trim(),
                middleName: middleName.trim() || null,
                phoneNumber: phoneNumber.trim() || null,
            });

            if (result.type === "error") {
                setError(result.error?.title || "Failed to create account.");
            } else {
                setSuccess(true);
                setFirstName("");
                setLastName("");
                setMiddleName("");
                setPhoneNumber("");
                if (onSuccess) onSuccess();
            }
        } catch {
            setError("An unexpected error occurred.");
        } finally {
            setIsCreating(false);
        }
    };
    return (
        <div>
            <h3>Create Account</h3>
            <form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="firstName">First Name *</label>
                    <input
                        id="firstName"
                        type="text"
                        value={firstName}
                        onChange={(e) => setFirstName(e.target.value)}
                        disabled={isCreating}
                    />
                </div>
                <div>
                    <label htmlFor="lastName">Last Name *</label>
                    <input
                        id="lastName"
                        type="text"
                        value={lastName}
                        onChange={(e) => setLastName(e.target.value)}
                        disabled={isCreating}
                    />
                </div>
                <div>
                    <label htmlFor="middleName">Middle Name (optional)</label>
                    <input
                        id="middleName"
                        type="text"
                        value={middleName}
                        onChange={(e) => setMiddleName(e.target.value)}
                        disabled={isCreating}
                    />
                </div>
                <div>
                    <label htmlFor="phoneNumber">Phone Number (optional)</label>
                    <input
                        id="phoneNumber"
                        type="tel"
                        value={phoneNumber}
                        onChange={(e) => setPhoneNumber(e.target.value)}
                        disabled={isCreating}
                    />
                </div>
                {error && <div>{error}</div>}
                {success && <div>Account created successfully!</div>}
                <button type="submit" disabled={isCreating}>
                    {isCreating ? "Creating..." : "Create Account"}
                </button>
            </form>
        </div>
    );
}