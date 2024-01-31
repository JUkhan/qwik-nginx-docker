import { component$, type Signal } from "@builder.io/qwik";
import { Form, routeAction$, routeLoader$, server$, z, zod$ } from "@builder.io/qwik-city";

export interface User {
    id:number,
    name: string;
    age:number
  }
  
  export let list: User[] = [
    {id:1, name:'jasim Khan', age:23}
  ];
  export const deleteUser = server$((id: number)=>{
    return list=list.filter(it=>it.id!==id);
  })
  export const useListLoader = routeLoader$(() => {
    console.log('pull user data')
    return list;
  });
  
  export const useAddOrUpdate = routeAction$(
    (item) => {
      if(item.id==0){
        item.id=Number(Date.now());
        list.push(item);
      } else {
        const temp=list.find(it=>it.id==item.id);
        if(!temp){
            return {
                success:false,
                message:'Not Found'
            }
        }
        temp.age=item.age;
        temp.name=item.name;
      } 
      
      return {
        success: true,
      };
    },
    zod$({
      id:z.coerce.number(),
      name: z.string().trim().min(3),
      age: z.coerce.number().gte(20)
    }),
  );
  
interface Props{
    editMode:Signal<boolean>,
    model:User
}
export const UserForm= component$<Props>(({editMode, model})=>{
    const action = useAddOrUpdate();
    return (<>
    <Form onSubmitCompleted$={()=>{
        if(action.value?.success){
            editMode.value=false;
        }
    }} action={action}>
        <input type="hidden" name="id" value={model.id}/>
       <div>
        <input placeholder="name" value={model.name} type="text" name="name"/>
        {action.value?.failed && <p>{action.value.fieldErrors.name}</p>}
        </div> 
       <div>
        <input placeholder="age" value={model.age} type="number" name="age"/>
        {action.value?.failed && <p>{action.value.fieldErrors.age}</p>}
        </div>
       <div>
        <button type="submit">Submit</button>
         <button type="button" onClick$={()=>editMode.value=false}>Cancel</button>
       </div>
    </Form>
   
    </>)
})