import {CatCharge} from './catCharge.model';
import {CatChargeDefaultAccount} from './catChargeDefaultAccount.model';

export class CatChargeToAddOrUpdate{
    charge : CatCharge = new CatCharge();
    listChargeDefaultAccount :CatChargeDefaultAccount[] = [];
}