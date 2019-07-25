import {Charge} from './catCharge.model';
import {CatChargeDefaultAccount} from './catChargeDefaultAccount.model';

export class CatChargeToAddOrUpdate{
    charge: Charge = new Charge();
    listChargeDefaultAccount :CatChargeDefaultAccount[] = [];
}