import {CatCharge} from './catCharge.model';
import {CatChargeDefaultAccount} from './catChargeDefaultAccount.model';

export class CatChargeToAddOrUpdate{
    charge : CatCharge;
    listChargeDefaultAccount : [CatCharge];
}