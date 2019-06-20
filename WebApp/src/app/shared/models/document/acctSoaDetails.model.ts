import { CsShipmentSurcharge } from "./csShipmentSurcharge";
import { AcctSOA } from './acctSoa.model';
export class AcctSoaDetails {
    PartnerNameEn: string;
    PartnerShippingAddress: string;
    PartnerTel: string;
    PartnerTaxcode: string;
    PartnerId: string;
    HbLadingNo: string;
    MbLadingNo: string;
    JobId: string;
    Pol: string;
    PolCountry: string;
    Pod: string;
    PodCountry: string;
    Vessel: string;
    HbConstainers: string;
    Etd: Date | string | null;
    Eta: Date | string | null;
    IsLocked: boolean;
    Volum: number | null;
    ListSurcharges: CsShipmentSurcharge[];
    Soa: AcctSOA;
}