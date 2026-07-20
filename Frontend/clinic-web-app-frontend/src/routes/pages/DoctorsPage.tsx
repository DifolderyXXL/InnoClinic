import {useEffect, useState} from "react";
import {profilesApi} from "../../services/api/ProfilesApi.ts";
import {PageSelector} from "./Shared/PageSelector.tsx";

const pageSize: number = 50;

export function DoctorsPage() {
    const [doctors, setDoctors] = useState<any>(null);
    const [total, setTotal] = useState<number>(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const loadData = async (page: number) => {
        setLoading(true);
        setError(null);

        try {
            const result = await profilesApi.getDoctors(page, pageSize);
            if (result.type === "ok") {
                setDoctors(result.value.items);
                setTotal(result.value.total);
            } else {
                setError(result.error?.title || "Error");
            }
        } catch (err) {
            setError("Unhandled error");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData(1);
    }, []);
    
    if (loading) {
        return <div style={{ textAlign: 'center', padding: '40px' }}>Loading doctors...</div>;
    }

    if (error) {
        return <div style={{ textAlign: 'center', padding: '40px', color: 'red' }}>{error}</div>;
    }

    const listItems = doctors.map(doctor =>
        <li>{doctor.id}</li>
    );
    
    return (
      <div style={{ display: 'flex', flexDirection: 'column', minWidth: '100vh', justifyContent: 'space-between'  }}>
          <div>
              {listItems}
          </div>
          <PageSelector pageSize={pageSize} total={total} onPageChange={(page) => { loadData(page) }}/>
      </div>  
    );
}