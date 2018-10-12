/**
 * This helper use to validate input from form such as email, phone number ...
 */

import * as validator from 'validator';

 /**
  * return true if email valid
  * return false if email invalid 
  * @param email 
  */
 export function ValidateEmail(email){
    return validator.isEmail(email);
 }

/**
 * return true if phone_number valid
 * return false if phone_number invalid 
 * @param phone_number 
 */
 export function ValidatePhoneNumber(phone_number){
     return validator.isMobilePhone(phone_number,'any');
 }

 