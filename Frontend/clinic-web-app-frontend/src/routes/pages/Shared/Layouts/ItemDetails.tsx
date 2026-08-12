import { useSearchParams} from "react-router";
import { useEffect, useState} from "react";

interface ItemDetailsProps<T = any> {
    provider: (id: string) => Promise<any>;
    extractor: (result: any) => T | null;
    children: (item: T) => React.ReactNode;
    onChange: (item: T|null) => void;
}

export function ItemDetails<T = any>({ provider, extractor, children, onChange }: ItemDetailsProps<T>) {
    const [searchParams] = useSearchParams();
    const [detail, setDetail] = useState<T | null>(null);
    const [loading, setLoading] = useState(true);

    const targetId = searchParams.get("id");

    useEffect(() => {
        if (!targetId) {
            setLoading(false);
            return;
        }

        setLoading(true);
        provider(targetId)
            .then((res) => {
                const item = extractor(res);
                setDetail(item);
                onChange?.(item);
            })
            .catch(() => setDetail(null))
            .finally(() => setLoading(false));
    }, [targetId]);

    if (!targetId) return <div className="status-message error">ID is missing</div>;
    if (loading) return <div className="status-message">Loading details...</div>;
    if (!detail) return <div className="status-message error">Not found</div>;

    return <div className="details-page">{children(detail)}</div>;
}