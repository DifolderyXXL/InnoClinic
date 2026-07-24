import {useEffect, useState} from "react";
import bffFetch from "../../../services/bffFetch.tsx";


const SAS_CACHE_KEY = "avatar_sas_cache";

function getSasCache(): Record<string, { url: string; expiresAt: number }> {
    try {
        const raw = localStorage.getItem(SAS_CACHE_KEY);
        return raw ? JSON.parse(raw) : {};
    } catch {
        return {};
    }
}

function setSasCache(cache: Record<string, { url: string; expiresAt: number }>) {
    try {
        localStorage.setItem(SAS_CACHE_KEY, JSON.stringify(cache));
    } catch (e) {
        console.warn("Failed to save SAS cache", e);
    }
}

interface AvatarProps{
    PhotoUrl?: string | null,
    TextIfPhotoNull: string,
    IsDirect?: boolean;
}

function getCachedPhotoUrl(PhotoUrl?: string | null, IsDirect?: boolean): string | null {
    if (!PhotoUrl) return null;
    if (IsDirect) return PhotoUrl;

    const cache = getSasCache();
    const cached = cache[PhotoUrl];
    if (cached && cached.expiresAt > Date.now()) {
        return cached.url;
    }
    return null;
}

export function AvatarFromSource({PhotoUrl, TextIfPhotoNull, IsDirect} : AvatarProps){
    const [photoSrc, setPhotoSrc] = useState<string | null>(()=>
        getCachedPhotoUrl(PhotoUrl, IsDirect)
    );
    
    useEffect(() => {
        if (!PhotoUrl) {
            setPhotoSrc(null);
            return;
        }

        if (IsDirect) {
            setPhotoSrc(PhotoUrl);
            return;
        }

        const cache = getSasCache();
        const cached = cache[PhotoUrl];
        const now = Date.now();

        if (cached && cached.expiresAt > now) {
            setPhotoSrc(cached.url);
            return;
        }

        bffFetch(PhotoUrl)
            .then(async (res) => {
                if (!res.ok) return null;
                return res.json();
            })
            .then(data => {
                if (!data || !data.url) {
                    setPhotoSrc(null);
                    return;
                }
                const hour = 1000*60*60;
                const expireWindow = data.expireTimeMillis ?? hour;
                const url = data.url;
                const expiresAt = now + (data.expireTimeMillis ?? expireWindow);
                
                const newCache = { ...cache, [PhotoUrl]: { url, expiresAt } };
                setSasCache(newCache);
                
                setPhotoSrc(data.url)
            })
            .catch(() => setPhotoSrc(null));
        
        
    }, [PhotoUrl, IsDirect]);
    
    return (
        <div className="avatar">
            {photoSrc ? (
                <img src={photoSrc} alt={TextIfPhotoNull} onError={() => setPhotoSrc(null)} />
            ) : (
                <div className="avatar-placeholder">{TextIfPhotoNull|| '?'}</div>
            )}
        </div>
    );
}