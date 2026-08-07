import {AccountCreateCard} from "../../Identity/AccountCard.tsx";
import {profilesApi} from "../../../../services/api/ProfilesApi.ts";

interface AdminExtra {
    email: string;
    roles: string[];
}

export function AccountCreatePage() {
    return (
    <AccountCreateCard<AdminExtra>
        title="Create Account (Admin)"
        extraInitialState={{ email: "", roles: ["client"] }}
        validateExtra={(extra) => {
            const errs: Record<string, string> = {};
            if (!extra.email.trim()) errs.email = "Please enter email";
            return errs;
        }}
        onCreate={(baseData, extraData) =>
            profilesApi.createAccount({
                ...baseData,
                email: extraData.email,
                roles: extraData.roles,
            })
        }
        renderExtraFields={({ extraData, setExtraData, isCreating, errors }) => (
            <div className="form-group">
                <label htmlFor="adminEmail">Email *</label>
                <input
                    id="adminEmail"
                    type="email"
                    className={errors.email ? "has-error" : ""}
                    value={extraData.email}
                    onChange={(e) =>
                        setExtraData((prev) => ({ ...prev, email: e.target.value }))
                    }
                    disabled={isCreating}
                />
                {errors.email && <span className="field-error-text">{errors.email}</span>}
            </div>
        )}
    />
    )
}