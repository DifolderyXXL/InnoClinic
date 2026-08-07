interface FormFieldProps {
    id: string;
    label: string;
    value?: string | null;
    error?: string;
    isTouched?: boolean;
    disabled?: boolean;
    onChange: (val: string) => void;
    onBlur?: () => void;
}

export function FormField({ id, label, value, error, isTouched, disabled, onChange, onBlur }: FormFieldProps) {
    const showError = isTouched && !!error;
    return (
        <div className="form-group">
            <label htmlFor={id}>{label}</label>
            <input
                id={id}
                type="text"
                className={showError ? "has-error" : ""}
                value={value ?? ""}
                onChange={(e) => onChange(e.target.value)}
                onBlur={onBlur}
                disabled={disabled}
            />
            {showError && <span className="field-error-text">{error}</span>}
        </div>
    );
}