
import * as lodash from 'lodash';

/**
 * Author: Thor The
 * Date: Mon, September 24, 2018
 */

/**
 * this function will return the list contain fields and their values ,
 * this list use to search item in reference_source list
 * @param current_list_fields_search 
 * @param list_fields 
 * @param key_search 
 * @param condition 
 */
export async function PrepareListFieldSearch(current_list_fields_search = null, list_fields, key_search, condition) {

    var list_fields_search: any[] = [];
    if (condition == 'and' || condition == 'AND') {
        list_fields_search = await PrepareListFieldsSearchWithAndCondition(current_list_fields_search, list_fields, key_search);
    }

    if (condition == 'or' || condition == 'OR') {
        list_fields_search = await PrepareListFieldsSearchWithOrCondition(list_fields, key_search);
    }

    return list_fields_search;

}

/**
 * This function return new list after filter reference_source follow list_keys_search and conditions
 * @param list_keys_search 
 * @param reference_source 
 * @param condition 
 */
export async function SearchEngine(list_keys_search: any[], reference_source: any[], condition) {
    var list_result: any[] = [];
    if (condition == 'and' || condition == 'AND') {
        list_result = await SearchWithAndCondition(list_keys_search, reference_source);
    }
    if (condition == 'or' || condition == 'OR') {
        list_result = await SearchWithOrCondition(list_keys_search, reference_source);
    }

    return list_result;

}





function SearchWithAndCondition(list_keys_search: any[], reference_source: any[]) {
    try {
        var ReturnList: any[];
        if (list_keys_search.length == 0) {
            ReturnList = reference_source;
        } else {
            ReturnList = lodash.filter(reference_source, function (o) {

                var result = false;
                var list_result: any[] = []
                for (var k = 0; k < list_keys_search.length; k++) {
                    var key = Object.keys(list_keys_search[k])[0];
                    var value = list_keys_search[k][key];
                    var property = eval("o." + key);
                    property = property == null ? "" : property;

                    if (typeof (value) == 'number' || typeof (value) == 'boolean') {
                        if (property.toString().toLowerCase() == (value.toString().toLowerCase())) {
                            result = true
                            list_result.push(result);
                        } else {
                            result = false;
                            list_result.push(result);
                        }
                    } else {
                        if (property.toString().toLowerCase().includes(value.toString().toLowerCase())) {
                            result = true
                            list_result.push(result);
                        } else {
                            result = false;
                            list_result.push(result);
                        }
                    }


                }
                if (lodash.uniq(list_result).length == 1 && list_result[0] == true) {
                    result = true;
                } else {
                    result = false;
                }

                return result;
            });
        }
        return ReturnList;
    } catch (error) {
        console.log(error);
    }
}



function SearchWithOrCondition(list_keys_search: any[], reference_source: any[]) {

    try {
        var ReturnList: any[];
        if (list_keys_search.length == 0) {
            ReturnList = reference_source;
        } else {
            ReturnList = lodash.filter(reference_source, function (o) {
                var result = false;
                for (var k = 0; k < list_keys_search.length; k++) {
                    var key = Object.keys(list_keys_search[k])[0];
                    var value = list_keys_search[k][key];
                    var property = eval("o." + key);
                    property = property == null ? "" : property;

                    if (typeof (value) == 'number' || typeof (value) == 'boolean') {
                        if (property.toString().toLowerCase() == (value.toString().toLowerCase())) {
                            result = result || true
                        }
                    } else {
                        if (property.toString().toLowerCase().includes(value.toString().toLowerCase())) {
                            result = result || true
                        }
                    }

                }

                return result;
            });
        }
        return ReturnList;

    } catch (error) {
        console.log(error);
    }

}


function PrepareListFieldsSearchWithAndCondition(current_list_fields_search, list_fields, key_search) {
    var index_existed = -1;

    for (var index in list_fields) {
        var key = list_fields[index];
        var value = key_search;
        var search_obj = {
            [key]: value
        }

        if (current_list_fields_search.length > 0) {

            for (var i = 0; i < current_list_fields_search.length; i++) {
                var obj_comp = Object.keys(current_list_fields_search[i])[0] === key;
                if (obj_comp) {
                    console.log(obj_comp);
                    index_existed = i;
                }
            }

            if (index_existed != -1 && value != "") {
                console.log(index_existed);
                current_list_fields_search.splice(index_existed, 1, search_obj);
            }
            if (index_existed != -1 && value == "") {
                current_list_fields_search.splice(index_existed, 1);
            }
            if (index_existed == -1) {
                current_list_fields_search.push(search_obj);
            }

        } else {
            current_list_fields_search.push(search_obj);
        }

        return current_list_fields_search;

    }
}

function PrepareListFieldsSearchWithOrCondition(list_fields, key_search) {
    var list_fields_search: any[] = [];
    if (key_search == "") {
        list_fields_search = [];
    } else {
        for (var index in list_fields) {
            var key = list_fields[index];
            var value = key_search;
            var search_obj = {
                [key]: value
            }
            list_fields_search.push(search_obj);
        }
    }

    return list_fields_search;
}



