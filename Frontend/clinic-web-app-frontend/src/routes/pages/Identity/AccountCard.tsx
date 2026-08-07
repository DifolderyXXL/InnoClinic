import React, { useState } from "react";
import "./AccountCard.css";
import { AccountEditCard } from "./AccountEditCard.tsx";
import type { Result } from "../../../services/api/BaseApiClient.ts";

interface AccountCardProps {
    email: string;
    phoneNumber: string;
    firstName: string;
    lastName: string;
    middleName?: string;
    photoUrl?: string | null;
    onUpdateSuccess?: () => void;
}

export function AccountCard({
                                email,
                                phoneNumber,
                                firstName,
                                lastName,
                                middleName,
                                photoUrl,
                                onUpdateSuccess,
                            }: AccountCardProps) {
    const [isEditing, setIsEditing] = useState(false);
    const fullName = [lastName, firstName, middleName].filter(Boolean).join(" ");

    const handleSuccess = () => {
        setIsEditing(false);
        if (onUpdateSuccess) onUpdateSuccess();
    };

    return (
        <div className="account-info-card">
            <header className="card-header">
                <h3>Account Details</h3>
                {!isEditing && (
                    <button
                        type="button"
                        className="edit-btn"
                        onClick={() => setIsEditing(true)}
                    >
                        Edit
                    </button>
                )}
            </header>

            {isEditing ? (
                <AccountEditCard
                    firstName={firstName}
                    lastName={lastName}
                    middleName={middleName}
                    phoneNumber={phoneNumber}
                    photoUrl={photoUrl}
                    onCancel={() => setIsEditing(false)}
                    onSuccess={handleSuccess}
                />
            ) : (
                <div className="account-details-body">
                    {photoUrl && (
                        <div className="account-photo-wrapper">
                            <img src={photoUrl} alt="Profile" className="account-avatar-img" />
                        </div>
                    )}
                    <div className="account-info-grid">
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
                </div>
            )}
        </div>
    );
}


export interface BaseAccountFormData {
    firstName: string;
    lastName: string;
    middleName: string | null;
    phoneNumber: string | null;
}

interface AccountCreateCardProps<TExtra = Record<string, unknown>> {
    title?: string;
    onCreate: (baseData: BaseAccountFormData, extraData: TExtra) => Promise<Result>;
    onSuccess?: () => void;
    extraInitialState?: TExtra;
    validateExtra?: (extraData: TExtra) => Record<string, string>;
    renderExtraFields?: (props: {
        extraData: TExtra;
        setExtraData: React.Dispatch<React.SetStateAction<TExtra>>;
        isCreating: boolean;
        errors: Record<string, string>;
    }) => React.ReactNode;
}

interface FormErrors {
    firstName?: string;
    lastName?: string;
    phoneNumber?: string;
    [key: string]: string | undefined;
}

interface TouchedFields {
    firstName?: boolean;
    lastName?: boolean;
    phoneNumber?: boolean;
    [key: string]: boolean | undefined;
}

export function AccountCreateCard<TExtra = Record<string, unknown>>({
                                                                        title = "Create Account",
                                                                        onSuccess,
                                                                        onCreate,
                                                                        extraInitialState = {} as TExtra,
                                                                        validateExtra,
                                                                        renderExtraFields,
                                                                    }: AccountCreateCardProps<TExtra>) {
    const [isCreating, setIsCreating] = useState(false);

    // Form fields
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [middleName, setMiddleName] = useState("");
    const [phoneNumber, setPhoneNumber] = useState("+");

    // Dynamic Extra state for extra fields (like Admin Email / Roles)
    const [extraData, setExtraData] = useState<TExtra>(extraInitialState);

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

        setTouched((prev) => ({
            ...prev,
            firstName: true,
            lastName: true,
            phoneNumber: true,
        }));

        const isFirstNameValid = validateField("firstName", firstName);
        const isLastNameValid = validateField("lastName", lastName);
        const isPhoneValid = validateField("phoneNumber", phoneNumber);

        // Полиморфная валидация внешних полей
        const extraErrors = validateExtra ? validateExtra(extraData) : {};
        const isExtraValid = Object.keys(extraErrors).length === 0;

        if (Object.keys(extraErrors).length > 0) {
            setErrors((prev) => ({ ...prev, ...extraErrors }));
        }

        if (!isFirstNameValid || !isLastNameValid || !isPhoneValid || !isExtraValid) {
            return;
        }

        setIsCreating(true);
        try {
            const baseData: BaseAccountFormData = {
                firstName: firstName.trim(),
                lastName: lastName.trim(),
                middleName: middleName.trim() || null,
                phoneNumber: phoneNumber.trim() || null,
            };

            // Делегируем выполнение стратегии через пропс onCreate
            const result = await onCreate(baseData, extraData);

            if (result.type === "error") {
                setApiError(result.error?.title || "Failed to create account.");
            } else {
                setSuccess(true);
                setFirstName("");
                setLastName("");
                setMiddleName("");
                setPhoneNumber("+");
                setExtraData(extraInitialState);
                setTouched({});
                setErrors({});
                if (onSuccess) onSuccess();
            }
        } catch(e) {
            console.log(e)
            setApiError("An unexpected error occurred.");
        } finally {
            setIsCreating(false);
        }
    };

    return (
        <div className="account-create-card">
            <header className="card-header">
                <h3>{title}</h3>
            </header>

            <form onSubmit={handleSubmit} className="account-create-form" noValidate>
                {renderExtraFields &&
                    renderExtraFields({
                        extraData,
                        setExtraData,
                        isCreating,
                        errors,
                    })}

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