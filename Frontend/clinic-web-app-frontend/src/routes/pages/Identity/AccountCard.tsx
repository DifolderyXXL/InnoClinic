import React, { useState } from "react";
import { profilesApi } from "../../../services/api/ProfilesApi.ts";
import "./AccountCard.css";

interface AccountCardProps {
    email: string;
    phoneNumber: string;
    firstName: string;
    lastName: string;
    middleName?: string;
}

export function AccountCard({ email, phoneNumber, firstName, lastName, middleName }: AccountCardProps) {
    const fullName = [lastName, firstName, middleName].filter(Boolean).join(" ");

    return (
        <div className="account-info-card">
            <div className="account-info-field">
                <span className="field-label">Name</span>
                <span className="field-value">{fullName}</span>
            </div>
            <div className="account-info-field">
                <span className="field-label">Email</span>
                <span className="field-value">{email}</span>
            </div>
            <div className="account-info-field">
                <span className="field-label">Phone</span>
                <span className="field-value">{phoneNumber || "—"}</span>
            </div>
        </div>
    );
}

interface AccountCreateCardProps {
    onSuccess?: () => void;
}

interface FormErrors {
    firstName?: string;
    lastName?: string;
    phoneNumber?: string;
}

interface TouchedFields {
    firstName?: boolean;
    lastName?: boolean;
    phoneNumber?: boolean;
}

export function AccountCreateCard({ onSuccess }: AccountCreateCardProps) {
    const [isCreating, setIsCreating] = useState(false);

    // Form fields
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [middleName, setMiddleName] = useState("");
    const [phoneNumber, setPhoneNumber] = useState("+");

    const [touched, setTouched] = useState<TouchedFields>({});
    const [errors, setErrors] = useState<FormErrors>({});
    const [apiError, setApiError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);

    const validateField = (name: string, value: string) => {
        let errorMsg = "";

        if (name === "firstName") {
            if (!value.trim()) errorMsg = "Please, enter the first name";
        }

        if (name === "lastName") {
            if (!value.trim()) errorMsg = "Please, enter the last name";
        }

        if (name === "phoneNumber") {
            const rawDigits = value.replace(/^\+/, "");
            if (!rawDigits.trim()) {
                errorMsg = "Please, enter the phone number";
            } else if (!/^\d+$/.test(rawDigits)) {
                errorMsg = "You've entered an invalid phone number";
            }
        }

        setErrors((prev) => ({ ...prev, [name]: errorMsg }));
        return !errorMsg;
    };

    const handleBlur = (fieldName: keyof TouchedFields, value: string) => {
        setTouched((prev) => ({ ...prev, [fieldName]: true }));
        validateField(fieldName, value);
    };

    const handlePhoneChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        let val = e.target.value;
        if (!val.startsWith("+")) {
            val = "+" + val.replace(/\+/g, "");
        }
        setPhoneNumber(val);
        if (touched.phoneNumber) {
            validateField("phoneNumber", val);
        }
    };

    const handleSubmit = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        setApiError(null);
        setSuccess(false);

        setTouched({
            firstName: true,
            lastName: true,
            phoneNumber: true,
        });

        const isFirstNameValid = validateField("firstName", firstName);
        const isLastNameValid = validateField("lastName", lastName);
        const isPhoneValid = validateField("phoneNumber", phoneNumber);

        if (!isFirstNameValid || !isLastNameValid || !isPhoneValid) {
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
                setApiError(result.error?.title || "Failed to create account.");
            } else {
                setSuccess(true);
                setFirstName("");
                setLastName("");
                setMiddleName("");
                setPhoneNumber("+");
                setTouched({});
                setErrors({});
                if (onSuccess) onSuccess();
            }
        } catch {
            setApiError("An unexpected error occurred.");
        } finally {
            setIsCreating(false);
        }
    };

    return (
        <div className="account-create-card">
            <header className="card-header">
                <h3>Create Account</h3>
            </header>

            <form onSubmit={handleSubmit} className="account-create-form" noValidate>
                <div className="form-grid name-grid">
                    {/* F-3 Last Name */}
                    <div className="form-group">
                        <label htmlFor="lastName">Last Name *</label>
                        <input
                            id="lastName"
                            type="text"
                            className={touched.lastName && errors.lastName ? "has-error" : ""}
                            value={lastName}
                            onChange={(e) => {
                                setLastName(e.target.value);
                                if (touched.lastName) validateField("lastName", e.target.value);
                            }}
                            onBlur={(e) => handleBlur("lastName", e.target.value)}
                            disabled={isCreating}
                        />
                        {touched.lastName && errors.lastName && (
                            <span className="field-error-text">{errors.lastName}</span>
                        )}
                    </div>

                    {/* F-2 First Name */}
                    <div className="form-group">
                        <label htmlFor="firstName">First Name *</label>
                        <input
                            id="firstName"
                            type="text"
                            className={touched.firstName && errors.firstName ? "has-error" : ""}
                            value={firstName}
                            onChange={(e) => {
                                setFirstName(e.target.value);
                                if (touched.firstName) validateField("firstName", e.target.value);
                            }}
                            onBlur={(e) => handleBlur("firstName", e.target.value)}
                            disabled={isCreating}
                        />
                        {touched.firstName && errors.firstName && (
                            <span className="field-error-text">{errors.firstName}</span>
                        )}
                    </div>

                    {/* F-4 Middle Name */}
                    <div className="form-group">
                        <label htmlFor="middleName">Middle Name (optional)</label>
                        <input
                            id="middleName"
                            type="text"
                            value={middleName}
                            onChange={(e) => setMiddleName(e.target.value)}
                            disabled={isCreating}
                        />
                    </div>
                </div>

                {/* F-5 Phone Number */}
                <div className="form-group">
                    <label htmlFor="phoneNumber">Phone Number *</label>
                    <input
                        id="phoneNumber"
                        type="text"
                        className={touched.phoneNumber && errors.phoneNumber ? "has-error" : ""}
                        value={phoneNumber}
                        onChange={handlePhoneChange}
                        onBlur={(e) => handleBlur("phoneNumber", e.target.value)}
                        disabled={isCreating}
                    />
                    {touched.phoneNumber && errors.phoneNumber && (
                        <span className="field-error-text">{errors.phoneNumber}</span>
                    )}
                </div>

                {apiError && <div className="status-message error">{apiError}</div>}
                {success && (
                    <div className="status-message success">Account created successfully!</div>
                )}

                <div className="form-actions">
                    <button type="submit" className="submit-btn" disabled={isCreating}>
                        {isCreating ? "Creating..." : "Create Account"}
                    </button>
                </div>
            </form>
        </div>
    );
}