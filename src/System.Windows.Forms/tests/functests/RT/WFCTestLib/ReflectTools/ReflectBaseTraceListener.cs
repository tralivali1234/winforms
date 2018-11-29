using System.Diagnostics;
namespace ReflectTools
{
    // <doc>
    // <desc>
    //  This class is used to capture the error messages from the assert dialog.
    // </desc>
    // </doc>
    //
    public class ReflectBaseTraceListener : TraceListener 
    {   	
    	ReflectBase rb ;
    	public ReflectBaseTraceListener(ReflectBase reflectBase)
    	{   	
    		rb = reflectBase ;	
    	}
    	
    	// <doc>
    	// <desc>
    	// Overriding the Fail method in the TraceListener to get the fail messages.
    	//<desc>
    	//<doc>
    	public override void Fail(string message, string detailMessage) {
            rb.SetAssertOrExceptionFailure( message, detailMessage );
            
        }  
        
        // <doc>
        // <desc>
        // We have to override this function because it's a abstart method in TraceListener.
        // </desc>
        // </doc>
        public override void Write(string message){
        }     
        
        // <doc>
        // <desc>
        // We have to override this function because it's a abstart method in TraceListener.
        // </desc>
        // </doc>
        public override void WriteLine(string message){
        }
    }
}    