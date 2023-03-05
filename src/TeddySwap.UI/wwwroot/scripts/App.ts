import { Kupmios, Lucid } from 'lucid-cardano';
import { Wallet } from './Types/Wallet';

declare global {
    interface Window {
        CardanoWalletService: {
            enableAsync: (walletId: string) => Promise<boolean>;
            getWallets: () => Wallet[];
            getAddressAsync: () => Promise<string | undefined>;
            lucid?: Lucid;
        }
    }
}

const getWallets = () => {
    const result: Wallet[] = [];
    const cardano = (window as any).cardano;
    for (const i in cardano) {
        const p = cardano[i];
        if (p.apiVersion != null && p.icon != null && p.name != null && i !== 'ccvault') {
            result.push({
                apiVersion: p.apiVersion,
                icon: p.icon,
                name: p.name,
                id: i.toString()
            });
        }
    }
    return result;
};

const enableAsync = async (walletId: string) => {
    try {
        const cardano = (window as any).cardano;
        const walletApi = await cardano[walletId].enable();
        window.CardanoWalletService.lucid = await Lucid.new(
            new Kupmios('https://kupo-preview-api-teddy-swap-preview-414e80.us1.demeter.run', 'wss://ogmios-preview-api-teddy-swap-preview-414e80.us1.demeter.run'),
            "Preview"
        );
        window.CardanoWalletService.lucid.selectWallet(walletApi);
    }
    catch (ex) {
        console.error(ex);
    }
    finally {
        return window.CardanoWalletService.lucid != undefined;
    }
}

const getAddressAsync = async () => {
    return await window.CardanoWalletService.lucid?.wallet.address()
};

window.CardanoWalletService = {
    getWallets,
    enableAsync,
    getAddressAsync
};