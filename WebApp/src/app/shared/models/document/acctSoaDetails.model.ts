import { CsShipmentSurcharge } from "./csShipmentSurcharge";
import { AcctSOA } from './acctSoa.model';
export class AcctSoaDetails {
    partnerNameEn: string = null ; 
    partnerShippingAddress: string = null ; 
    partnerTel: string = null ; 
    partnerTaxcode: string = null ; 
    partnerId: string = null ; 
    hbLadingNo: string = null ; 
    mbLadingNo: string = null ; 
    jobId: string = null ; 
    pol: string = null ; 
    polName: string = null ; 
    polCountry: string = null ; 
    pod: string = null ; 
    podName: string = null ; 
    podCountry: string = null ; 
    vessel: string = null ; 
    hbConstainers: string = null;
    etd: Date = null;
    eta: Date = null;
    isLocked: boolean;
    volum: number = null;
    listSurcharges: CsShipmentSurcharge[]=[];
    soa: AcctSOA = null;
    totalCredit: number = null;
    totalDebit: number = null;

}